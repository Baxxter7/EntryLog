# EntryLog

Solución empresarial en .NET 8 para el registro y control de jornadas laborales. Incluye una aplicación web MVC para gestionar usuarios, Face ID, check-in/check-out y sesiones de trabajo con persistencia híbrida en SQL Server y MongoDB.

## Tabla de contenidos

- [Características](#características)
- [Arquitectura](#arquitectura)
- [Stack tecnológico](#stack-tecnológico)
- [Requisitos previos](#requisitos-previos)
- [Instalación](#instalación)
- [Configuración](#configuración)
- [Uso](#uso)
- [Endpoints principales](#endpoints-principales)
- [Patrones y prácticas](#patrones-y-prácticas)
## Características

- Registro de sesiones de trabajo (apertura y cierre).
- Manejo de estados de sesión (`InProgress`, `Completed`).
- Captura de evidencia por imagen y datos de ubicación.
- Registro y consulta de Face ID por empleado.
- Autenticación por cookies en la aplicación web.
- Generación y validación de token JWT para flujos internos de Face ID.
- Persistencia híbrida:
  - **SQL Server**: datos legacy de empleados.
  - **MongoDB**: usuarios de aplicación, sesiones y Face ID.

## Arquitectura

El proyecto sigue una arquitectura en capas con separación de responsabilidades:

| Proyecto | Descripción |
|----------|-------------|
| `EntryLog.Web` | Aplicación web ASP.NET Core MVC (autenticación por cookies, vistas Razor, JavaScript). |
| `EntryLog.Business` | Lógica de aplicación, DTOs, mappers, seguridad, integraciones externas. |
| `EntryLog.Data` | Acceso a datos (MongoDB + EF Core SQL Server legacy). |
| `EntryLog.Entities` | Entidades POCO y enumeraciones compartidas. |

## Stack tecnológico

| Categoría | Tecnología |
|-----------|------------|
| Framework | .NET 8 (`net8.0`) |
| Lenguaje | C# 12 con nullable reference types |
| Web | ASP.NET Core MVC |
| ORM | EF Core 8 (SQL Server) |
| Base de datos NoSQL | MongoDB Driver |
| Patrones | Repository, Specification |
| Seguridad | Argon2 (hash de contraseñas), JWT, RSA |
| Imagenes | ImageBB API |
| Email | Mailtrap API |
| Frontend | Face API JS |

## Requisitos previos

- [.NET SDK 8.x](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019+ (para datos legacy de empleados)
- MongoDB 6.0+ (para datos de aplicación)

## Instalación

```bash
git clone <url-del-repositorio>
cd EntryLog
dotnet restore EntryLog.slnx
```

## Configuración

Configura los valores por ambiente usando `appsettings.Development.json`, variables de entorno o `dotnet user-secrets`.

### Variables de configuración

| Sección | Clave | Descripción |
|---------|-------|-------------|
| ConnectionStrings | EmployeeDB | Cadena de conexión SQL Server |
| EntryLogDbOptions | ConnectionUri | URI de conexión MongoDB |
| EntryLogDbOptions | DatabaseName | Nombre de base de datos MongoDB |
| ImageBBOptions | ApiUrl, ApiToken | Configuración de almacenamiento de imágenes |
| MailtrapApiOptions | ApiUrl, ApiToken, FromEmail | Configuración de envío de emails |
| Argon2PasswordHashOptions | DegreeOfParallelism, MemorySize, etc. | Parámetros de hash de contraseñas |
| EncryptionKeyValues | PublicKey, PrivateKey | Claves RSA para cifrado |
| JwtConfiguration | Secret | Secreto para tokens JWT |

### Ejemplo de configuración

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

## Uso

### Compilar la solución

```bash
dotnet build EntryLog.slnx
```

### Ejecutar la aplicación web

```bash
dotnet run --project EntryLog.Web/EntryLog.Web.csproj
```

### URLs de desarrollo

- HTTPS: `https://localhost:7179`
- HTTP: `http://localhost:5171`

### Tests

```bash
dotnet test EntryLog.slnx
```

## Endpoints principales

### AccountController (`/account`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/register-employee` | Registro de empleado |
| POST | `/login` | Inicio de sesión |
| POST | `/recovery-start` | Iniciar recuperación de cuenta |
| POST | `/recovery-complete` | Completar recuperación de cuenta |

### WorkSessionController (`/worksession`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/open` | Abrir sesión de trabajo |
| POST | `/close` | Cerrar sesión de trabajo |
| POST | `/filter` | Filtrar sesiones |

### FaceIdController (`/faceid`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Consultar Face ID |
| POST | `/register` | Registrar Face ID |

## Patrones y prácticas

- **Repository pattern**: Abstracción del acceso a datos.
- **Specification pattern**: Consultas complejas reutilizables.
- **DTOs (records)**: Contratos de entrada/salida inmutables.
- **Guard clauses**: Validación temprana con early return.
- **Dependency Injection**: Configuración centralizada en `BusinessDependencies` y `DataDependencies`.
- **Nullable reference types**: Tipado explícito de valores opcionales.