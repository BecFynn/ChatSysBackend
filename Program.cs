using System.Reflection;
using ChatSysBackend.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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
//builder.Services.AddSingleton<WebSocketManager>();

var app = builder.Build();


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

app.Run();