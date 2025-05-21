using System.Net;
using System.Reflection;
using ChatSysBackend.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net.WebSockets;
using System.Security.Claims;
using ChatSysBackend.Controllers.Config;
using ChatSysBackend.Database.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

builder.Configuration.AddEnvironmentVariables("APP_");

var config = new AppConfig();
builder.Configuration.Bind(config);
builder.Services.AddSingleton(config);

builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseMySql(config.Database.ConnectionString,
        MySqlServerVersion.LatestSupportedServerVersion));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, opt =>
    {
        opt.Cookie.Name = "Manager.Auth";

        opt.Cookie.IsEssential = true;
        opt.ExpireTimeSpan = TimeSpan.FromHours(10);

        opt.Cookie.Domain = null;

        opt.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        opt.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddOpenIdConnect("bosch", "Bosch", opt =>
    {
        opt.MetadataAddress = config.OAuth.MetaDataAddress;
        opt.GetClaimsFromUserInfoEndpoint = true;
        opt.ClientId = config.OAuth.ClientId;
        opt.ClientSecret = config.OAuth.ClientSecret;
        opt.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
        opt.SignInScheme = IdentityConstants.ExternalScheme;
        opt.CallbackPath = "/api/v1/signin-oidc";
        foreach (var scope in config.OAuth.Scopes.Split(",").Select(x => x.Trim()))
            opt.Scope.Add(scope);
    }).AddCookie(IdentityConstants.ExternalScheme, opt => { opt.Cookie.Name = "Manager.External"; });

var ib = builder.Services.AddIdentityCore<User>(opt =>
{
    opt.SignIn.RequireConfirmedAccount = false;
    opt.User.RequireUniqueEmail = true;
    opt.User.AllowedUserNameCharacters =
        "@abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-.";
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireLowercase = false;

    opt.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
}).AddEntityFrameworkStores<DataContext>()
.AddRoles<UserRole>()
.AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<User, UserRole>>()
.AddDefaultTokenProviders()
.AddSignInManager();

builder.Services.AddScoped<IRoleStore<UserRole>, RoleStore<UserRole, DataContext, Guid>>();
builder.Services.AddScoped<IUserStore<User>, UserStore<User, UserRole, DataContext, Guid>>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddCors(o => o.AddPolicy("DevCors", b =>
    {
        b.WithOrigins(["http://127.0.0.1:5173", "http://localhost:5173"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    }));


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
    app.UseCors("DevCors");
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