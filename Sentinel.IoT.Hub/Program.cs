using Microsoft.AspNetCore.Server.Kestrel.Core;
using Sentinel.IoT.Hub.Data;
using Sentinel.IoT.Hub.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Console.BackgroundColor = ConsoleColor.DarkRed;
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine(">>> MODO NUCLEAR: SIN SEGURIDAD (HTTP) <<<");
Console.ResetColor();

// CONFIGURACIÓN KESTREL SIN HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    // Escuchamos en 5001 pero SIN certificado
    options.ListenAnyIP(5001, listenOptions =>
    {
        // gRPC necesita HTTP/2 obligatoriamente
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Base de datos y Servicios
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<TelemetryService>();
app.MapGet("/", () => "Sentinel IoT Hub - INSECURE MODE");

app.Run();