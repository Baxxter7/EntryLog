# 🕒 EntryLog – Sistema de Registro de Asistencia

EntryLog es un sistema diseñado para el **registro, control y gestión de jornadas laborales** de empleados. Permite administrar **entradas, salidas, ubicaciones, métodos de marcaje y sesiones de trabajo**, soportando tanto persistencia relacional como NoSQL.

El proyecto está construido con **ASP.NET Core (.NET 8)** y sigue una arquitectura limpia, orientada a dominio, preparada para escalar y adaptarse a diferentes motores de base de datos.

---

## 🧩 Funcionalidades Principales

* Registro de **Check-In** y **Check-Out** de empleados
* Control de **sesiones de trabajo** (Work Sessions)
* Manejo de estados de sesión (InProgress, Closed, etc.)
* Registro de **ubicación** (latitud, longitud, IP)
* Asociación de **usuarios de aplicación** (AppUser)
* Soporte para múltiples métodos de marcaje (dispositivo, app, etc.)
* Persistencia híbrida: **SQL Server + MongoDB**

---

## 🏗️ Arquitectura

El proyecto sigue un enfoque **Clean Architecture / Domain Driven Design (DDD)**:

* **Domain**: Entidades, Value Objects, Enums y reglas de negocio
* **Application / Business**: DTOs, servicios y casos de uso
* **Data**: Repositorios, DbContexts, Mongo Collections, Serializadores
* **API**: Endpoints REST y configuración del host(Por definir)

---

## 🔧 Tecnologías Utilizadas

* **.NET 8 / ASP.NET Core**
* **C# 12**
* **Entity Framework Core** (SQL Server)
* **MongoDB Driver** (NoSQL)
* **Specification Pattern**
* **Dependency Injection** nativa de ASP.NET Core
* **SonarQube / SonarLint** (calidad de código)

---

## 📦 Requisitos Previos

Antes de ejecutar el proyecto, asegúrate de tener:

* ✅ **.NET SDK 8**
* ✅ **SQL Server** (local o remoto)
* ✅ **MongoDB**

---

## 🚀 Configuración del Proyecto

### 1️⃣ Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd EntryLog
```

---

### 2️⃣ Configuración de `appsettings.json`

Ejemplo:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=EntryLogDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "MongoDb": "mongodb://localhost:27017"
  },
  "MongoOptions": {
    "DatabaseName": "EntryLog"
  }
}
```

---

### 3️⃣ Migraciones (SQL Server)

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## ▶️ Ejecución

Desde la carpeta del proyecto API:

```bash
dotnet run
```

El servicio quedará disponible en:

```
https://localhost:7026;http://localhost:5110
```

---

## 📁 Estructura del Proyecto

```
EntryLog
├── EntryLog.API
│   ├── Controllers
│   ├── Program.cs
│   └── appsettings.json
├── EntryLog.Domain
│   ├── Entities
│   ├── Enums
│   └── ValueObjects
├── EntryLog.Business
│   ├── DTOs
│   ├── Services
│   └── Specifications
├── EntryLog.Data
│   ├── SqlServer
│   │   ├── DbContexts
│   │   └── Repositories
│   ├── Mongo
│   │   ├── Collections
│   │   └── BsonSerializers
│   └── DependencyInjection
└── EntryLog.sln
```

---

## 🛠️ Patrones y Buenas Prácticas

* Repository Pattern
* Specification Pattern
* Records para DTOs
* Separación clara de capas
* AsNoTracking para consultas de solo lectura
* Serialización personalizada para MongoDB
