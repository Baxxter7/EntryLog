# EntryLog

EntryLog es una solucion en .NET 8 para registro y control de jornadas laborales.
Incluye una API REST y una aplicacion web MVC para gestionar usuarios, Face ID,
check-in/check-out y sesiones de trabajo con persistencia hibrida en SQL Server y MongoDB.

## Tabla de contenidos

- [Caracteristicas](#caracteristicas)
- [Arquitectura](#arquitectura)
- [Stack tecnologico](#stack-tecnologico)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Requisitos previos](#requisitos-previos)
- [Instalacion rapida](#instalacion-rapida)
- [Configuracion](#configuracion)
- [Build, run y test](#build-run-y-test)
- [Endpoints principales de API](#endpoints-principales-de-api)
- [Flujo web (MVC)](#flujo-web-mvc)
- [Patrones y practicas](#patrones-y-practicas)
- [Buenas practicas de seguridad](#buenas-practicas-de-seguridad)
- [Roadmap tecnico sugerido](#roadmap-tecnico-sugerido)

## Caracteristicas

- Registro de sesiones de trabajo (apertura y cierre).
- Manejo de estados de sesion (`InProgress`, `Completed`).
- Captura de evidencia por imagen y datos de ubicacion.
- Registro y consulta de Face ID por empleado.
- Autenticacion por cookies en la app web.
- Generacion y validacion de token JWT para flujos internos de Face ID.
- Persistencia en:
  - SQL Server (datos legacy de empleados).
  - MongoDB (usuarios de aplicacion, sesiones y Face ID).

## Arquitectura

El proyecto sigue una separacion por capas:

- `EntryLog.Api`: API ASP.NET Core (controllers + Swagger).
- `EntryLog.Web`: Web MVC ASP.NET Core (cookies, vistas, JS).
- `EntryLog.Business`: logica de aplicacion, DTOs, mappers, seguridad, integraciones.
- `EntryLog.Data`: acceso a datos (MongoDB + EF Core SQL legacy).
- `EntryLog.Entities`: entidades POCO y enums compartidos.

## Stack tecnologico

- .NET 8 (`net8.0`)
- C# 12
- ASP.NET Core Web API + MVC
- C# (nullable reference types habilitado)
- EF Core 8 (SQL Server)
- MongoDB Driver
- Specification pattern
- Repository pattern
- Dependency Injection nativa
- Argon2 para hash de contrasenas
- Swagger (Swashbuckle)
- Face API JS (frontend)

## Requisitos previos

- .NET SDK 8.x
- SQL Server accesible
- MongoDB accesible
- (Opcional) Visual Studio 2022 o VS Code

## Instalacion rapida

```bash
git clone <url-del-repositorio>
cd EntryLog
dotnet restore EntryLog.slnx
```

## Configuracion

Configura valores por ambiente usando `appsettings.Development.json`, variables de
entorno o `dotnet user-secrets`.

### Claves de configuracion esperadas

- `ConnectionStrings:EmployeeDB`
- `EntryLogDbOptions:ConnectionUri`
- `EntryLogDbOptions:DatabaseName`
- `ImageBBOptions:ApiUrl`
- `ImageBBOptions:ApiToken`
- `ImageBBOptions:ExpirationSeconds`
- `MailtrapApiOptions:ApiUrl`
- `MailtrapApiOptions:ApiToken`
- `MailtrapApiOptions:FromEmail`
- `MailtrapApiOptions:FromName`
- `MailtrapApiOptions:Templates`
- `Argon2PasswordHashOptions:*`
- `EncryptionKeyValues:PublicKey`
- `EncryptionKeyValues:PrivateKey`
- `JwtConfiguration:Secret`

### Ejemplo minimo (referencial)

```json
{
  "ConnectionStrings": {
    "EmployeeDB": "Server=localhost,1433;Database=EmpleadosBD;User Id=<user>;Password=<password>;TrustServerCertificate=True;"
  },
  "EntryLogDbOptions": {
    "ConnectionUri": "mongodb://<user>:<password>@localhost:27017",
    "DatabaseName": "ENTRY_LOG"
  },
  "JwtConfiguration": {
    "Secret": "<secret>"
  }
}
```

## Build, run y test

### Restore

```bash
dotnet restore EntryLog.slnx
```

### Build solucion completa

```bash
dotnet build EntryLog.slnx
```

### Ejecutar API

```bash
dotnet run --project EntryLog.Api/EntryLog.Api.csproj
```

Swagger disponible en entorno Development:

- `https://localhost:7026/swagger`
- `http://localhost:5110/swagger`

### Ejecutar Web MVC

```bash
dotnet run --project EntryLog.Web/EntryLog.Web.csproj
```

URL local (Development):

- `https://localhost:7179`
- `http://localhost:5171`

### Tests

Actualmente no hay proyectos de test en la solucion.

Cuando existan:

```bash
dotnet test EntryLog.slnx
```

## Endpoints principales de API

Base URL: `api/[controller]`

### AccountController (`/api/account`)

- `POST /register-employee`
  - Body JSON: `CreateEmployeeUserDto`
- `POST /login`
  - Body JSON: `UserCredentialsDto`
- `POST /recovery-start`
  - Body JSON: `string` (username/email)
- `POST /recovery-complete`
  - Body JSON: `AccountRecoveryDto`

### JobSessionController (`/api/jobsession`)

- `POST /open`
  - Content-Type: `multipart/form-data`
  - Form: `CreateJobSessionDto`
- `POST /close`
  - Content-Type: `multipart/form-data`
  - Form: `CloseJobSessionDto`
- `POST /filter`
  - Query: `WorkSessionQueryFilter`

## Flujo web (MVC)

Controladores principales en `EntryLog.Web/Controllers`:

- `AccountController`: login y registro de empleado.
- `MainController`: menu principal (rol Employee).
- `FaceIdController`: consulta y alta de Face ID (rol Employee).
- `HomeController`: pagina base.

La aplicacion usa autenticacion por cookie (`CookieAuthenticationDefaults.AuthenticationScheme`).

## Patrones y practicas

- Repository pattern + Specification pattern para consultas.
- DTOs (records) para contratos de entrada/salida.
- Separacion clara por capas (`Api`, `Web`, `Business`, `Data`, `Entities`).
- Guard clauses y early return en servicios.
- Uso de `DateTime.UtcNow` para timestamps del servidor.

## Buenas practicas de seguridad

- No subas secretos reales al repositorio.
- Usa variables de entorno o secret manager en desarrollo.
- Rota credenciales si alguna fue expuesta.
- Manten separados los valores de `Development`, `QA` y `Production`.
- Revisa politicas de CORS, autenticacion y autorizacion antes de despliegue productivo.

## Roadmap tecnico sugerido

- Agregar pruebas unitarias para servicios de negocio.
- Agregar pruebas de integracion para endpoints de login y sesiones.
- Estandarizar respuestas HTTP de error con `ProblemDetails`.
- Endurecer validaciones de entrada (DTOs y guard clauses).
- Incorporar pipeline CI (build + test + analisis estatico).
