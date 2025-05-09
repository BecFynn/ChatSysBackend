using System.Net;
using System.Reflection;
using ChatSysBackend.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseMySql("Server=localhost;Database=chatsys;Port=3306;User=test;Password=test;",
        MySqlServerVersion.LatestSupportedServerVersion));
builder.Services.AddAutoMapper(opt => { opt.AddMaps(Assembly.GetExecutingAssembly()); });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    
    opt.MapType<DateOnly>((() => new OpenApiSchema()
    {
        Type = "string",
        Format = "date"
    })) ;
});
builder.Services.AddSingleton<WebsocketManager>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddCors(o => o.AddPolicy("DevCors", b =>
    {
        b.WithOrigins(["http://127.0.0.1:5173", "http://localhost:5173"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    }));


var app = builder.Build();

app.UseWebSockets();
var webSocketManager = app.Services.GetRequiredService<WebsocketManager>();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        webSocketManager.AddSocket(ws);

        while (ws.State == WebSocketState.Open)
        {
        }

        webSocketManager.RemoveClosedSockets();
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}


Console.WriteLine("Applying pending migrations...");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dbContext.Database.Migrate();
}

Console.WriteLine("Applied pending migrations...");


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors("DevCors");
app.Run();