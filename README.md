# Sentinel IoT Hub

A high-performance IoT telemetry system built with gRPC and .NET 10, designed for secure and efficient data collection from remote sensors.

## Overview

Sentinel IoT Hub is a client-server solution that enables real-time telemetry data collection from IoT devices. The system consists of two main components:

- **Hub**: A gRPC server that receives and stores telemetry data in SQL Server
- **Sensor**: A simulated IoT client that sends sensor readings (temperature, pressure) to the Hub

## Architecture

```
┌─────────────────┐         gRPC (HTTP/2)        ┌─────────────────┐
│                 │ ────────────────────────────> │                 │
│  IoT Sensor     │                               │  IoT Hub        │
│  (Client)       │ <──────────────────────────── │  (Server)       │
│                 │         MetricAck             │                 │
└─────────────────┘                               └─────────────────┘
                                                          │
                                                          │ Entity Framework
                                                          │ Core
                                                          ▼
                                                  ┌─────────────────┐
                                                  │  SQL Server     │
                                                  │  (LocalDB)      │
                                                  └─────────────────┘
```

### Components

- **Sentinel.IoT.Hub**: gRPC server application that exposes telemetry endpoints and persists data to SQL Server
- **Sentinel.IoT.Sensor**: Simulated IoT device that periodically sends sensor readings to the Hub

## Technology Stack

- **.NET 10.0**: Latest .NET framework
- **gRPC**: High-performance RPC framework for telemetry communication
- **Entity Framework Core**: ORM for database operations
- **SQL Server LocalDB**: Local database for development
- **Protocol Buffers**: Efficient serialization format for gRPC messages

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [SQL Server LocalDB](https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio)
- Visual Studio 2022 or VS Code (recommended)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/MaxAcosta-30/SentinelIoT.git
cd SentinelIoT
```

### 2. Database Setup

The application uses SQL Server LocalDB by default. Ensure LocalDB is installed and running.

The connection string is configured in `Sentinel.IoT.Hub/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SentinelIoTDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Run Database Migrations

Navigate to the Hub project directory and apply migrations:

```bash
cd Sentinel.IoT.Hub
dotnet ef database update
```

If you need to create a new migration:

```bash
dotnet ef migrations add <MigrationName>
```

### 4. Start the Hub (Server)

In the `Sentinel.IoT.Hub` directory:

```bash
dotnet run
```

The Hub will start listening on `http://localhost:5001` for gRPC connections.

### 5. Start the Sensor (Client)

In a new terminal, navigate to the `Sentinel.IoT.Sensor` directory:

```bash
cd Sentinel.IoT.Sensor
dotnet run
```

The sensor will start sending telemetry data every 2 seconds to the Hub.

## Project Structure

```
SentinelIoT/
├── Sentinel.IoT.Hub/          # gRPC Server
│   ├── Data/                  # Entity Framework models and DbContext
│   │   ├── AppDbContext.cs
│   │   └── TelemetryLog.cs
│   ├── Services/             # gRPC service implementations
│   │   └── TelemetryService.cs
│   ├── Protos/               # Protocol Buffer definitions
│   │   └── telemetry.proto
│   ├── Migrations/           # EF Core database migrations
│   └── Program.cs
├── Sentinel.IoT.Sensor/       # gRPC Client (Simulated IoT Device)
│   └── Program.cs
└── README.md
```

## Configuration

### Hub Configuration

The Hub can be configured via `appsettings.json`:

- **Connection String**: Database connection settings
- **Logging**: Log levels for different components
- **Kestrel**: Server port and protocol configuration (currently HTTP/2 on port 5001)

### Security Notes

⚠️ **Current Status**: The application is configured to use **HTTP (insecure)** for development purposes only.

**Future Implementation**: Mutual TLS (mTLS) will be implemented for production security, including:
- Server certificate configuration
- Client certificate validation
- Certificate-based device authentication

See `TODO` comments in the codebase for planned security enhancements.

## Development

### Building the Solution

```bash
dotnet build
```

### Running Tests

(Add test project information when available)

### Code Style

- Follow C# coding conventions
- Use XML documentation comments for public APIs
- Maintain clean, professional logging (avoid debug-only console output)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is open source. (Specify license when available)

## Author

**MaxAcosta-30**

- GitHub: [@MaxAcosta-30](https://github.com/MaxAcosta-30)

## Acknowledgments

Built with modern .NET technologies for high-performance IoT telemetry collection.

---

**Note**: This is an MVP (Minimum Viable Product) version. Future enhancements include production-grade security, monitoring, and scalability features.

