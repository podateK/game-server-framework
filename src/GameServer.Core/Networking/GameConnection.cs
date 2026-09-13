using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace GameServer.Core.Networking
{
    public sealed class GameConnection : IAsyncDisposable
    {
        private readonly Socket _socket;
        private readonly Pipe _sendPipe;
        private readonly Pipe _receivePipe;
        private readonly SocketAsyncEventArgs _sendArgs;
        private readonly SocketAsyncEventArgs _receiveArgs;
        private readonly Memory<byte> _receiveBuffer;
        private CancellationTokenSource _cts;
        private bool _isConnected;
        private long _bytesSent;
        private long _bytesReceived;

        public Guid Id { get; }
        public EndPoint RemoteEndPoint => _socket.RemoteEndPoint!;
        public bool IsConnected => _isConnected && _socket.Connected;
        public long BytesSent => Interlocked.Read(ref _bytesSent);
        public long BytesReceived => Interlocked.Read(ref _bytesReceived);

        public PipeReader ReceiveReader => _receivePipe.Reader;
        public PipeWriter SendWriter => _sendPipe.Writer;

        public event Func<GameConnection, ReadOnlySequence<byte>, ValueTask>? OnDataReceived;
        public event Func<GameConnection, ValueTask>? OnDisconnected;
        public event Func<GameConnection, Exception, ValueTask>? OnError;

        private const int DefaultBufferSize = 8192;

        public GameConnection(Socket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            Id = Guid.NewGuid();
            _cts = new CancellationTokenSource();
            _isConnected = true;

            _sendPipe = new Pipe(new PipeOptions(
                poolSize: 1024,
                minimumSegmentSize: 1024,
                maximumSizeHigh: 1024 * 1024,
                maximumSizeLow: DefaultBufferSize));

            _receivePipe = new Pipe(new PipeOptions(
                poolSize: 1024,
                minimumSegmentSize: 1024,
                maximumSizeHigh: 1024 * 1024,
                maximumSizeLow: DefaultBufferSize));

            _receiveBuffer = new byte[DefaultBufferSize];

            _sendArgs = new SocketAsyncEventArgs();
            _sendArgs.Completed += OnSendCompleted;
            _sendArgs.SetBuffer(new byte[DefaultBufferSize], 0, DefaultBufferSize);

            _receiveArgs = new SocketAsyncEventArgs();
            _receiveArgs.SetBuffer(_receiveBuffer);
            _receiveArgs.Completed += OnReceiveCompleted;
        }

        public ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
            var token = linkedCts.Token;

            _ = Task.Factory.StartNew(() => SendLoopAsync(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _ = Task.Factory.StartNew(() => ReceiveLoopAsync(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _ = Task.Factory.StartNew(() => ProcessReceivedDataAsync(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return ValueTask.CompletedTask;
        }

        public ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
                return ValueTask.CompletedTask;

            return _sendPipe.Writer.WriteAsync(data, cancellationToken);
        }

        public ValueTask DisconnectAsync()
        {
            if (!Interlocked.CompareExchange(ref _isConnected, false, true))
                return ValueTask.CompletedTask;

            _cts.Cancel();

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            try
            {
                _socket.Close();
            }
            catch
            {
            }

            _sendPipe.Writer.CancelPendingFlush();
            _receivePipe.Reader.CancelPendingRead();
            _sendPipe.Reset();
            _receivePipe.Reset();

            return OnDisconnected?.Invoke(this) ?? ValueTask.CompletedTask;
        }

        private async Task SendLoopAsync(CancellationToken cancellationToken)
        {
            Exception? error = null;
            try
            {
                await foreach (var flushResult in _sendPipe.Reader.ReadAllAsync(cancellationToken))
                {
                    if (!IsConnected) break;

                    var sequence = flushResult.Buffer;
                    if (sequence.Length == 0) continue;

                    var position = sequence.Start;
                    while (sequence.TryGet(ref position, out var memory))
                    {
                        if (memory.Length == 0) continue;

                        if (!await SendMemoryAsync(memory, cancellationToken))
                        {
                            error = new SocketException((int)SocketError.ConnectionReset);
                            break;
                        }
                    }

                    _sendPipe.Reader.AdvanceTo(sequence.End);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                if (error != null && OnError != null)
                {
                    await OnError.Invoke(this, error);
                }
                await DisconnectAsync();
            }
        }

        private async Task<bool> SendMemoryAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            var totalSent = 0;
            while (totalSent < data.Length)
            {
                var segment = data[totalSent..];
                var sent = await _socket.SendAsync(segment, SocketFlags.None, cancellationToken);
                if (sent == 0) return false;
                totalSent += sent;
                Interlocked.Add(ref _bytesSent, sent);
            }
            return true;
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            Exception? error = null;
            try
            {
                while (!cancellationToken.IsCancellationRequested && IsConnected)
                {
                    var result = await _socket.ReceiveAsync(_receiveBuffer, SocketFlags.None, cancellationToken);
                    if (result == 0) break;

                    Interlocked.Add(ref _bytesReceived, result);

                    var flushResult = await _receivePipe.Writer.WriteAsync(_receiveBuffer.AsMemory(0, result), cancellationToken);
                    if (flushResult.IsCanceled || flushResult.IsCompleted) break;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.ConnectionAborted)
            {
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                _receivePipe.Writer.Complete(error);
                _sendPipe.Reader.Complete(error);
            }
        }

        private async Task ProcessReceivedDataAsync(CancellationToken cancellationToken)
        {
            Exception? error = null;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await _receivePipe.Reader.ReadAsync(cancellationToken);
                    if (result.IsCanceled || result.IsCompleted) break;

                    var buffer = result.Buffer;
                    if (buffer.Length > 0 && OnDataReceived != null)
                    {
                        await OnDataReceived.Invoke(this, buffer);
                    }

                    _receivePipe.Reader.AdvanceTo(buffer.End);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                _receivePipe.Reader.Complete(error);
            }
        }

        private void OnSendCompleted(object? sender, SocketAsyncEventArgs e)
        {
        }

        private void OnReceiveCompleted(object? sender, SocketAsyncEventArgs e)
        {
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();

            _sendArgs.Completed -= OnSendCompleted;
            _receiveArgs.Completed -= OnReceiveCompleted;
            _sendArgs.Dispose();
            _receiveArgs.Dispose();

            _sendPipe.Reader.Complete();
            _sendPipe.Writer.Complete();
            _receivePipe.Reader.Complete();
            _receivePipe.Writer.Complete();

            _cts.Dispose();
            _socket.Dispose();
        }
    }
}
