using Grpc.Core;
using Sentinel.IoT.Hub.Data; // Importamos tu capa de datos
using Microsoft.EntityFrameworkCore;

namespace Sentinel.IoT.Hub.Services;

public class TelemetryService : Telemetry.TelemetryBase
{
    private readonly ILogger<TelemetryService> _logger;
    private readonly AppDbContext _dbContext; // Tu conexión a SQL

    // Inyectamos el Logger y la Base de Datos
    public TelemetryService(ILogger<TelemetryService> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<MetricAck> SendMetrics(MetricData request, ServerCallContext context)
    {
        try 
        {
            // 1. Convertir Timestamp Unix a DateTime
            var time = DateTimeOffset.FromUnixTimeSeconds(request.TimestampUtc).DateTime;

            // 2. [SEGURIDAD] Intentar leer quién es realmente el cliente
            var httpContext = context.GetHttpContext();
            var clientCert = httpContext.Connection.ClientCertificate;
            
            // Si no hay certificado (aún no configuramos Kestrel), pondremos "Anonymous" temporalmente
            string verifiedId = clientCert?.Subject ?? "ANONYMOUS_DEV_MODE";

            _logger.LogInformation($"Recibiendo datos de: {request.DeviceId} | Temp: {request.Temperature}");

            // 3. Crear la entidad para guardar en SQL Server
            var log = new TelemetryLog
            {
                DeviceId = request.DeviceId,
                VerifiedIdentity = verifiedId, 
                Temperature = request.Temperature,
                Pressure = request.Pressure,
                Timestamp = time
            };

            // 4. Guardar en BD (Async para no bloquear el servidor)
            _dbContext.TelemetryLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            return new MetricAck { Success = true, Message = "Data stored securely." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando telemetría");
            return new MetricAck { Success = false, Message = "Internal Server Error" };
        }
    }
}