# EntryLog — Proyecto de Pruebas Unitarias

## Objetivo

Agregar cobertura de tests unitarios al proyecto EntryLog usando xUnit con enfoque London School (mock-first). El proyecto `EntryLog.Api` queda excluido ya que será eliminado.

## Cambios Requeridos en Código Fuente

Varios servicios, mappers e interfaces están marcados como `internal`. Para que el proyecto de tests pueda acceder a ellos, se debe agregar `InternalsVisibleTo` en cada `.csproj` afectado:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="EntryLog.Tests" />
</ItemGroup>
```

Proyectos que lo requieren:
- `EntryLog.Business` (servicios, mappers, crypto, JWT)
- `EntryLog.Data` (si tiene clases internal que necesiten testing)

Además, se debe agregar el proyecto a `EntryLog.slnx`.

## Target Framework

El proyecto de tests usa `net8.0` para coincidir con el resto de la solución.

## Estructura

Un proyecto de tests: `EntryLog.Tests` ubicado en `tests/EntryLog.Tests/`.

```
tests/
└── EntryLog.Tests/
    ├── EntryLog.Tests.csproj
    ├── Helpers/                — Test data builders y fixtures
    ├── Business/
    │   ├── Services/          — Tests de cada servicio (mock-first)
    │   ├── Cryptography/      — Tests de Argon2, RSA (concretos, con IOptions)
    │   ├── JWT/               — Tests de generación/validación tokens (concretos)
    │   └── Mappers/           — Tests de mappers DTO<->Entity
    │── Data/
    │   └── Specifications/    — Tests de specs contra colecciones in-memory
    └── Web/
        └── Controllers/       — Tests de controllers MVC con mocks + DefaultHttpContext
```

## Paquetes NuGet

| Paquete | Propósito |
|---------|-----------|
| xunit | Framework de testing |
| xunit.runner.visualstudio | Runner para VS/CLI |
| Microsoft.NET.Test.Sdk | Infraestructura de tests |
| NSubstitute | Mocking de dependencias |
| FluentAssertions | Assertions legibles |
| coverlet.collector | Cobertura de código |

## Enfoque de Testing

### London School (Mock-First) — para Services y Controllers

- Cada unidad se testea en **aislamiento total**.
- Las dependencias (repositorios, otros servicios) se reemplazan con mocks usando NSubstitute.
- Se verifica comportamiento (interacciones con dependencias), no solo estado.

### Tests Concretos — para Crypto y JWT

Los componentes de criptografía y JWT son hojas (leaf dependencies) que realizan operaciones reales. No se mockean: se testean con implementaciones concretas usando `Options.Create(new XxxOptions { ... })` con valores de prueba.

- Argon2: requiere opciones de hashing válidas
- RSA: requiere pares de llaves RSA pre-generados en los fixtures
- JWT: requiere un secret válido en `JwtConfiguration`

### Tests de Specifications — contra colecciones in-memory

Las specifications usan `Expression<Func<T, bool>>` que se compilan y evalúan contra listas en memoria. No se mockean expression trees.

### Convenciones

- **Nombres de tests**: `MetodoQueSeTestea_Escenario_ResultadoEsperado`
  - Ejemplo: `LoginAsync_CredencialesValidas_RetornaToken`
  - Ejemplo: `HashPassword_InputVacio_LanzaArgumentException`
- **Patrón AAA**: Arrange, Act, Assert en cada test.
- **Un assert por test** cuando sea posible (excepto validaciones de objetos complejos).

## Helpers y Test Data Builders

Carpeta `Helpers/` para builders de entidades complejas reutilizables:
- `AppUserBuilder` — construye `AppUser` con defaults sensatos
- `WorkSessionBuilder` — construye `WorkSession` con `Check`, `Location`
- `EmployeeBuilder` — construye `Employee`

Esto evita repetir la construcción de objetos complejos en cada test.

## Prioridad de Cobertura

### 1. Services (Alta prioridad)

Donde está la lógica de negocio. Cada servicio en `EntryLog.Business/Services/` tendrá su clase de test correspondiente con mocks de los repositorios que inyecta.

### 2. Cryptography y JWT (Alta prioridad)

Componentes de seguridad críticos (tests concretos, no mock-first):
- Argon2 password hashing: verificar hash y validación
- RSA encryption: cifrar/descifrar con llaves de prueba
- JWT: generación y validación de tokens con secret de prueba

### 3. Controllers Web (Media prioridad)

Los controllers de `EntryLog.Web/Controllers/` se testean mockeando los servicios. Requieren:
- `DefaultHttpContext` para simular HttpContext
- Mock de `IAuthenticationService` para cookie authentication (`SignInCookiesAsync`)
- Verificar tipos de resultado (ViewResult, RedirectResult)
- Verificar llamadas a servicios correctos

### 4. Mappers y Specifications (Baja prioridad)

- Mappers: verificar transformaciones DTO <-> Entity
- Specifications: compilar expressions y evaluar contra colecciones in-memory

## Alcance Explícito — Fuera de Scope

Los siguientes componentes quedan **diferidos a tests de integración**:
- `ImageBBService` (wrapper HTTP a ImgBB API)
- `MailtrapApiService` (wrapper HTTP a Mailtrap)
- `UriService` (construcción de URIs con HttpContext)

Estos dependen de `HttpClient`/`IHttpClientFactory` y se benefician más de tests de integración.

## Proyectos Referenciados

`EntryLog.Tests` referenciará:
- `EntryLog.Business`
- `EntryLog.Data`
- `EntryLog.Entities`
- `EntryLog.Web`

## Ejecución

```bash
# Correr todos los tests
dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj

# Con cobertura
dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --collect:"XPlat Code Coverage"

# Filtrar por categoría
dotnet test --filter "FullyQualifiedName~Business.Services"
```

## Decisiones

- **Sin tests de integración** por ahora. Se agregarán como segundo paso con Testcontainers.
- **Sin proyecto `EntryLog.Api`** — será eliminado, no se incluye en la cobertura.
- **FluentAssertions** para assertions más legibles que las de xUnit nativo.
- **NSubstitute** sobre Moq por sintaxis más limpia y sin controversia de telemetría.
- **InternalsVisibleTo** para acceder a clases internal desde el proyecto de tests.
- **Test data builders** para evitar duplicación en la construcción de entidades complejas.
