using Microsoft.EntityFrameworkCore;

namespace Sentinel.IoT.Hub.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TelemetryLog> TelemetryLogs { get; set; }
}