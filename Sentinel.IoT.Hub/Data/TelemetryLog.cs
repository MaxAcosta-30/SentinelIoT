using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sentinel.IoT.Hub.Data;

[Index(nameof(Timestamp))] // Índice para búsquedas rápidas por fecha
public class TelemetryLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string DeviceId { get; set; } = string.Empty;

    // Aquí guardaremos la identidad real extraída del certificado
    [Required]
    [MaxLength(200)]
    public string VerifiedIdentity { get; set; } = string.Empty;

    public double Temperature { get; set; }
    public double Pressure { get; set; }

    public DateTime Timestamp { get; set; }
}