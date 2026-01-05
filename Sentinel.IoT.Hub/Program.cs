using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using Sentinel.IoT.Hub.Data;
using Sentinel.IoT.Hub.Services;

/// <summary>
/// Punto de entrada principal del Sentinel IoT Hub.
/// Configura el servidor gRPC con autenticación mTLS (Mutual TLS) y la base de datos SQLite.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("[INFO] Sentinel IoT Hub - Iniciando servidor gRPC con mTLS...");

// Cargar certificados X.509 para mTLS
var baseDir = AppContext.BaseDirectory;
var rootCertPath = Path.Combine(baseDir, "Certs", "MyRootCA.crt");
var serverCertPath = Path.Combine(baseDir, "Certs", "Server.pfx");

if (!File.Exists(rootCertPath))
{
    Console.WriteLine($"[ERR] Certificado Root CA no encontrado: {rootCertPath}");
    return;
}

if (!File.Exists(serverCertPath))
{
    Console.WriteLine($"[ERR] Certificado del servidor no encontrado: {serverCertPath}");
    return;
}

var rootCert = X509CertificateLoader.LoadCertificateFromFile(rootCertPath);
var serverCert = X509CertificateLoader.LoadPkcs12FromFile(serverCertPath, "sentinel", X509KeyStorageFlags.EphemeralKeySet);

Console.WriteLine("[INFO] Certificados cargados correctamente");

// Configurar Kestrel para gRPC con mTLS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
        listenOptions.UseHttps(serverCert, httpsOptions =>
        {
            // Permitir certificados de cliente para autenticación mutua
            httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
            
            // Validación personalizada del certificado del cliente
            // Verificamos que el certificado esté firmado por nuestra Root CA
            httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
            {
                if (cert == null || chain == null) return true;
                
                // Configurar la cadena de confianza para usar nuestra Root CA personalizada
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.Add(rootCert);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                if (!chain.Build(cert)) 
                {
                    Console.WriteLine($"[WARN] Certificado rechazado - cadena inválida: {cert.Subject}");
                    return false;
                }

                // Verificar que el certificado raíz de la cadena coincida con nuestra Root CA
                // Esto previene ataques de suplantación usando certificados de otras CAs
                if (chain.ChainElements.Count == 0)
                {
                    Console.WriteLine($"[WARN] Certificado rechazado - cadena vacía: {cert.Subject}");
                    return false;
                }

                var chainRoot = chain.ChainElements[^1].Certificate;
                if (chainRoot.Thumbprint != rootCert.Thumbprint)
                {
                    Console.WriteLine($"[WARN] Certificado rechazado - Root CA no coincide: {cert.Subject}");
                    return false;
                }

                Console.WriteLine($"[INFO] Cliente autenticado: {cert.Subject}");
                return true;
            };
        });
    });
});

// Configurar servicios
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=sentinel_linux.db"));
builder.Services.AddGrpc();

var app = builder.Build();

// Registrar servicio gRPC
app.MapGrpcService<TelemetryService>();

// Endpoint REST para consultar telemetría
app.MapGet("/api/telemetry", async (AppDbContext db) => 
{
    var data = await db.TelemetryLogs
        .OrderByDescending(x => x.Timestamp)
        .Take(20)
        .ToListAsync();
        
    return Results.Ok(data);
});

Console.WriteLine("[INFO] Servidor iniciado en https://localhost:5001");
Console.WriteLine("[INFO] Endpoint REST disponible en: https://localhost:5001/api/telemetry");

app.Run();