using Microsoft.EntityFrameworkCore;

namespace Sentinel.IoT.Hub.Data;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos SQLite del Sentinel IoT Hub.
/// Gestiona el acceso a los registros de telemetría almacenados.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Inicializa una nueva instancia del contexto de base de datos.
    /// </summary>
    /// <param name="options">Opciones de configuración del contexto (cadena de conexión, proveedor, etc.).</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// Conjunto de entidades que representa los registros de telemetría almacenados en la base de datos.
    /// </summary>
    public DbSet<TelemetryLog> TelemetryLogs { get; set; }
}