using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sentinel.IoT.Hub.Data;

/// <summary>
/// Representa un registro de telemetría almacenado en la base de datos.
/// Incluye la identidad verificada del dispositivo mediante certificado X.509 para auditoría de seguridad.
/// </summary>
[Index(nameof(Timestamp))]
public class TelemetryLog
{
    /// <summary>
    /// Identificador único del registro de telemetría (clave primaria).
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Identificador del dispositivo IoT que envió los datos.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Identidad verificada extraída del certificado X.509 del cliente.
    /// Este campo es crítico para la auditoría de seguridad y garantiza que solo dispositivos autorizados puedan enviar datos.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string VerifiedIdentity { get; set; } = string.Empty;

    /// <summary>
    /// Temperatura medida por el sensor (en grados Celsius).
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    /// Presión medida por el sensor (en unidades estándar).
    /// </summary>
    public double Pressure { get; set; }

    /// <summary>
    /// Marca de tiempo (timestamp) cuando se recibieron los datos de telemetría.
    /// </summary>
    public DateTime Timestamp { get; set; }
}