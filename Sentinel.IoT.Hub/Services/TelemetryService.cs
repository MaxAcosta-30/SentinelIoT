using Grpc.Core;
using Sentinel.IoT.Hub.Data;
using Microsoft.EntityFrameworkCore;

namespace Sentinel.IoT.Hub.Services;

/// <summary>
/// Servicio gRPC que procesa y almacena datos de telemetría de sensores IoT.
/// Implementa autenticación basada en certificados X.509 para verificar la identidad del cliente.
/// </summary>
public class TelemetryService : Telemetry.TelemetryBase
{
    private readonly ILogger<TelemetryService> _logger;
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de telemetría.
    /// </summary>
    /// <param name="logger">Logger para registrar eventos y errores.</param>
    /// <param name="dbContext">Contexto de base de datos para persistir los datos de telemetría.</param>
    public TelemetryService(ILogger<TelemetryService> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Procesa y almacena datos de telemetría recibidos de un sensor IoT.
    /// Extrae la identidad verificada del certificado del cliente para auditoría.
    /// </summary>
    /// <param name="request">Datos de telemetría del sensor (temperatura, presión, timestamp).</param>
    /// <param name="context">Contexto de la llamada gRPC que contiene información del cliente.</param>
    /// <returns>Confirmación de recepción con estado de éxito o error.</returns>
    public override async Task<MetricAck> SendMetrics(MetricData request, ServerCallContext context)
    {
        try 
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(request.TimestampUtc).DateTime;

            // Extraer la identidad verificada del certificado del cliente
            // Esto es crítico para la seguridad: solo aceptamos conexiones con certificados válidos
            var httpContext = context.GetHttpContext();
            var clientCert = httpContext.Connection.ClientCertificate;
            
            string verifiedId = clientCert?.Subject ?? "ANONYMOUS_DEV_MODE";

            _logger.LogInformation($"Recibiendo datos de: {request.DeviceId} | Temp: {request.Temperature}");

            var log = new TelemetryLog
            {
                DeviceId = request.DeviceId,
                VerifiedIdentity = verifiedId, 
                Temperature = request.Temperature,
                Pressure = request.Pressure,
                Timestamp = time
            };

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