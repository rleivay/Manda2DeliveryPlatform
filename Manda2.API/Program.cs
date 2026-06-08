using Manda2.API.DependencyInjection;
using Manda2.API.Hubs;
using Manda2.API.Middleware;
using Manda2.API.Services;
using Manda2.Application;
using Manda2.Application.Common;
using Manda2.Application.Extensions;
using Manda2.Application.Mediator;
using Manda2.Application.Services;
using Manda2.Infrastructure.BackgroundServices;
using Manda2.Infrastructure.Persistence;
using Manda2.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// 1. BASE DE DATOS
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Manda2.Infrastructure")));

builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

// 2. CONTROLADORES
builder.Services.AddControllers();
builder.Services.AddApplicationServices();
builder.Services.AddDispatchModule();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddHostedService<DispatchWorker>();
builder.Services.AddHttpContextAccessor();

//2.1 CONFIGURACIÓN DE SIGNALR
builder.Services.AddSignalR(options =>
{
    // Tiempo máximo sin actividad antes de cerrar la conexión
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
builder.Services.AddSingleton<IDriverLocationCache, InMemoryDriverLocationCache>();
builder.Services.AddScoped<IHubNotificationService, HubNotificationService>();

// 3. SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Manda2 API",
        Version = "v1",
        Description = "API Backend para plataforma de Delivery"
    });
});

// 4. ✅ MANDA2 MEDIATOR (reemplaza Scrutor + MediatR)
builder.Services.AddManda2Mediator(
    typeof(IApplicationAssemblyMarker).Assembly
);

//4.1 Add JWD Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero  // Sin tolerancia de tiempo
    };
});

builder.Services.AddAuthorization();

builder.Services.AddHttpClient("AzureMaps");

// 5. PIPELINE HTTP
var app = builder.Build();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Manda2 API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseStaticFiles(); // Sirve wwwroot/** sin autenticación

// Opcional: si quieres que /uploads/* tenga cache headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache de 1 hora para imágenes
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=3600";
    }
});

app.UseAuthentication();
app.UseAuthorization();

// Hub para notificaciones en tiempo real (ej: nuevos pedidos, actualizaciones de estado)
app.MapHub<Manda2Hub>("/hubs/manda2");

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();
app.Run();