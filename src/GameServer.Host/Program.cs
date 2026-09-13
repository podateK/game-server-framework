using Microsoft.EntityFrameworkCore;
using GameServer.Core;
using GameServer.Core.Persistence;
using GameServer.Core.Rooms;
using GameServer.Host.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=gameserver.db"));

builder.Services.AddSingleton<LiteDbPersistence>(sp => new LiteDbPersistence("gameserver.litedb"));
builder.Services.AddSingleton<RoomManager>();
builder.Services.AddSingleton<GameHost>(sp =>
{
    var ip = builder.Configuration["GameServer:Ip"] ?? "127.0.0.1";
    var port = int.Parse(builder.Configuration["GameServer:Port"] ?? "7777");
    return new GameHost(ip, port);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors();
app.MapControllers();
app.MapHub<GameHub>("/gamehub");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    dbContext.Database.EnsureCreated();
}

var gameHost = app.Services.GetRequiredService<GameHost>();
gameHost.Start();

app.Lifetime.ApplicationStopping.Register(() => gameHost.Stop());

app.Run();
