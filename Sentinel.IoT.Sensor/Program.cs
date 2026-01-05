using Grpc.Net.Client;
using System.Security.Cryptography.X509Certificates;
using Sentinel.IoT.Hub;

/// <summary>
/// Cliente simulado de sensor IoT que envía telemetría al Hub mediante gRPC con autenticación mTLS.
/// </summary>
Console.WriteLine("[INFO] Sentinel IoT Sensor - Iniciando cliente gRPC con mTLS...");

var baseDir = AppContext.BaseDirectory;
var rootCaPath = Path.Combine(baseDir, "Certs", "MyRootCA.crt");
var clientCertPath = Path.Combine(baseDir, "Certs", "Sensor01.pfx");

if (!File.Exists(rootCaPath))
{
    Console.WriteLine($"[ERR] Certificado Root CA no encontrado: {rootCaPath}");
    return;
}

if (!File.Exists(clientCertPath))
{
    Console.WriteLine($"[ERR] Certificado del cliente no encontrado: {clientCertPath}");
    return;
}

// Cargar certificados
var rootCert = X509CertificateLoader.LoadCertificateFromFile(rootCaPath);
var clientCert = X509CertificateLoader.LoadPkcs12FromFile(clientCertPath, "sentinel", X509KeyStorageFlags.EphemeralKeySet);

Console.WriteLine("[INFO] Certificados cargados correctamente");

// Configurar handler HTTP con certificado de cliente para mTLS
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(clientCert);

// Validación del certificado del servidor
// Verificamos que el servidor esté autenticado con un certificado firmado por nuestra Root CA
handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
{
    if (cert == null || chain == null) return false;
    
    chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
    chain.ChainPolicy.CustomTrustStore.Add(rootCert);
    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
    
    bool isValid = chain.Build(cert);
    
    if (!isValid || chain.ChainElements.Count == 0)
    {
        return false;
    }
    
    bool isTrustedRoot = chain.ChainElements[^1].Certificate.Thumbprint == rootCert.Thumbprint;
    
    return isValid && isTrustedRoot;
};

// Establecer conexión gRPC con el Hub
using var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { HttpHandler = handler });
var client = new Telemetry.TelemetryClient(channel);
var random = new Random();

Console.WriteLine("[INFO] Conectado al Hub. Iniciando envío de telemetría...");

// Bucle principal: enviar datos de telemetría cada 2 segundos
while (true)
{
    try
    {
        var data = new MetricData 
        { 
            DeviceId = "SENSOR-LINUX-01", 
            Temperature = 300 + random.Next(10), 
            TimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds() 
        };

        Console.Write($"[INFO] Enviando telemetría: {data.Temperature}°C... ");
        var reply = await client.SendMetricsAsync(data);
        
        Console.WriteLine($"OK - {reply.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERR] Error al enviar telemetría: {ex.Message}");
    }
    
    await Task.Delay(2000);
}