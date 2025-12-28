using Grpc.Net.Client;
using Sentinel.IoT.Hub;

Console.Title = "Sentinel Sensor - MODO NUCLEAR";

// --- TRUCO VITAL: Permitir HTTP/2 sin encriptar (Solo para desarrollo) ---
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

Console.WriteLine(">>> INICIANDO SENSOR (SIN SSL) <<<");
Console.WriteLine("Conectando a http://localhost:5001 ...");

// Nota que ahora es HTTP (no HTTPS)
using var channel = GrpcChannel.ForAddress("http://localhost:5001");
var client = new Telemetry.TelemetryClient(channel);

var random = new Random();

while (true)
{
    try
    {
        var data = new MetricData
        {
            DeviceId = "SENSOR-NUCLEAR",
            Temperature = 500 + random.Next(100), // Temperatura alta para celebrar
            Pressure = 2000,
            TimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        Console.Write($"Enviando {data.Temperature}C... ");
        var reply = await client.SendMetricsAsync(data);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[ÉXITO] Hub respondió: {reply.Message}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FALLO] {ex.Message}");
        Console.ResetColor();
    }

    await Task.Delay(2000);
}