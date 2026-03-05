# EntryLog - Sistema de Registro de Asistencia

EntryLog es un sistema para registro y control de jornadas laborales. Incluye
API REST y sitio MVC para gestionar entradas/salidas, ubicacion, sesiones de
trabajo y datos de empleados, con persistencia SQL Server y MongoDB.

---

## Funcionalidades principales

- Registro de check-in y check-out.
- Control de sesiones de trabajo (Work Sessions).
- Manejo de estados de sesion (InProgress, Completed).
- Registro de ubicacion (latitud, longitud, IP) y foto.
- Autenticacion por cookies en Web y JWT para flujos internos (Face ID).
- Persistencia hibrida: SQL Server (empleados) + MongoDB (usuarios/sesiones).

---

## Arquitectura y proyectos

- `EntryLog.Api`: API ASP.NET Core (controllers, Swagger).
- `EntryLog.Web`: MVC ASP.NET Core (cookies, session, views).
- `EntryLog.Business`: servicios, DTOs, mappers, auth, mail.
- `EntryLog.Data`: repositorios MongoDB y SQL legacy (EF Core).
- `EntryLog.Entities`: entidades POCO y enums.
- `EntryLog.slnx`: solucion para build/test.

---

## Tecnologias

- .NET 8 / ASP.NET Core
- C# 12
- Entity Framework Core (SQL Server)
- MongoDB Driver
- Specification pattern
- Dependency Injection nativa

---

## Requisitos previos

- .NET SDK 8
- SQL Server accesible
- MongoDB accesible

---

## Configuracion

### 1) Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd EntryLog
```

### 2) Configurar `appsettings.json`

Las claves esperadas estan en `EntryLog.Api/appsettings.json` y
`EntryLog.Web/appsettings.json`. Para entornos locales, usa
`appsettings.Development.json` con tus valores reales.

Ejemplo (valores de muestra):

```json
{
  "ConnectionStrings": {
    "EmployeeDB": "Server=localhost,1433;Database=EmpleadosBD;User Id=USER;Password=PASSWORD;TrustServerCertificate=True;"
  },
  "EntryLogDbOptions": {
    "ConnectionUri": "mongodb://USER:PASSWORD@localhost:27017",
    "DatabaseName": "ENTRY_LOG"
  },
  "ImageBBOptions": {
    "ApiUrl": "https://api.imgbb.com",
    "ApiToken": "<token>",
    "ExpirationSeconds": 0
  },
  "MailtrapApiOptions": {
    "ApiUrl": "https://send.api.mailtrap.io",
    "ApiToken": "<token>",
    "FromEmail": "hello@demomailtrap.co",
    "FromName": "Entry Log",
    "Templates": [
      { "Uuid": "<uuid>", "Name": "RecoveryToken" }
    ]
  },
  "Argon2PasswordHashOptions": {
    "DegreeOfParallelism": 8,
    "MemorySize": 65536,
    "Iterations": 4,
    "SaltSize": 16,
    "HashSize": 32
  },
  "EncryptionKeyValues": {
    "PublicKey": "<RSAKeyValue>",
    "PrivateKey": "<RSAKeyValue>"
  },
  "JwtConfiguration": {
    "Secret": "<secret>"
  }
}
```

Nota: evita commitear secretos. Usa `appsettings.Development.json` para
credenciales locales.

---

## Build, run y test

### Restore

```bash
dotnet restore EntryLog.slnx
```

### Build

```bash
dotnet build EntryLog.slnx
```

### Run

```bash
dotnet run --project EntryLog.Api/EntryLog.Api.csproj
```

```bash
dotnet run --project EntryLog.Web/EntryLog.Web.csproj
```

### Tests

No hay proyectos de pruebas en este repo. Si se agregan:

```bash
dotnet test EntryLog.slnx
```

---

## Estructura del proyecto

```
EntryLog
├── EntryLog.Api
│   ├── Controllers
│   ├── Program.cs
│   └── appsettings.json
├── EntryLog.Web
│   ├── Controllers
│   ├── Views
│   ├── wwwroot
│   └── Program.cs
├── EntryLog.Business
│   ├── DTOs
│   ├── Interfaces
│   ├── Services
│   └── Mappers
├── EntryLog.Data
│   ├── MongoDB
│   ├── SqlLegacy
│   └── Specifications
├── EntryLog.Entities
│   ├── POCOEntities
│   └── Enums
└── EntryLog.slnx
```

---

## Patrones y practicas

- Repository pattern
- Specification pattern
- DTOs con records
- Separacion clara de capas
- Guard clauses y early return en servicios
- Fecha/hora en UTC (`DateTime.UtcNow`)
