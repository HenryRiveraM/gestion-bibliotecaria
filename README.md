# StarBook Library — Sistema de Gestión Bibliotecaria

Aplicación web para la administración de bibliotecas, desarrollada con ASP.NET Core Razor Pages y MySQL. Permite gestionar libros, autores, ejemplares y usuarios del sistema dentro de una sola plataforma.

---

## Tabla de Contenidos

- [Tecnologías](#tecnologías)
- [Arquitectura](#arquitectura)
- [Funcionalidades](#funcionalidades)
- [Requisitos Previos](#requisitos-previos)
- [Configuración](#configuración)
- [Base de Datos](#base-de-datos)
- [Cómo Ejecutar](#cómo-ejecutar)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Roles de Usuario](#roles-de-usuario)

---

## Tecnologías

| Tecnología        | Versión  | Uso                          |
|-------------------|----------|------------------------------|
| .NET              | 10.0     | Framework principal          |
| ASP.NET Core      | 10.0     | Razor Pages (UI + servidor)  |
| MySQL             | 8+       | Base de datos relacional     |
| MySql.Data        | 9.6.0    | Conector ADO.NET para MySQL  |
| Bootstrap         | 5        | Estilos y componentes UI     |
| jQuery            | 3        | Interactividad en el cliente |

---

## Arquitectura

El proyecto sigue una arquitectura limpia (Clean Architecture) organizada en cuatro capas:

```
Domain          → Entidades, puertos (interfaces de repositorios), errores, validaciones
Aplicacion      → Servicios de aplicación e interfaces de servicio
Infrastructure  → Repositorios (Persistence), seguridad, correo, configuración
Pages           → Capa de presentación (Razor Pages)
```

### Diagrama de capas

```
Pages (UI)
   │
   ▼
Aplicacion (Servicios)
   │
   ▼
Domain (Entidades / Interfaces)
   ▲
   │
Infrastructure (Repositorios / Seguridad / Correo)
```

---

## Funcionalidades

### Implementadas

| Módulo     | Operaciones                              | Acceso          |
|------------|------------------------------------------|-----------------|
| Libros     | Crear, editar, eliminar, listar          | Autenticado     |
| Autores    | Crear, editar, eliminar, listar          | Autenticado     |
| Ejemplares | Crear, editar, eliminar, listar          | Autenticado     |
| Usuarios   | Crear, editar, eliminar, listar          | Solo Admin      |
| Sesión     | Inicio y cierre de sesión               | Público         |

### Planificadas (en desarrollo)

- **Circulación**: Préstamos, devoluciones, reservas, historial, multas.
- **Reportes**: Préstamos realizados, libros más prestados, ejemplares disponibles, usuarios con retraso.
- **Catálogo extendido**: Categorías y editoriales.

---

## Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [MySQL 8+](https://dev.mysql.com/downloads/) en ejecución local (o servidor accesible)
- Un cliente MySQL para ejecutar los scripts de migración (por ejemplo, MySQL Workbench, DBeaver o `mysql` CLI)

---

## Configuración

### 1. Cadena de conexión

Edita `gestion-bibliotecaria/appsettings.json` y ajusta la cadena de conexión a tu instancia de MySQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=bibliotecabd;User=root;Password=TU_PASSWORD;"
  }
}
```

> Para entornos de desarrollo, también puedes sobreescribir la cadena en `appsettings.Development.json` o mediante [User Secrets](https://learn.microsoft.com/es-es/aspnet/core/security/app-secrets):
> ```bash
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=...;"
> ```

### 2. Correo electrónico (opcional)

El sistema soporta tres modos de envío de correos:

| Modo              | Descripción                                    |
|-------------------|------------------------------------------------|
| `Development`     | No envía correos; muestra los mensajes en log  |
| `Smtp`            | Envío directo por SMTP (por ejemplo, Gmail)    |
| `HttpApi`         | Envío mediante una API HTTP externa            |

Configura la sección `Email` en `appsettings.json`:

```json
"Email": {
  "UseDevelopmentMode": true,
  "Provider": "Smtp",
  "FromName": "Sistema Bibliotecario",
  "FromAddress": "tu_correo@ejemplo.com",
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "tu_correo@ejemplo.com",
    "Password": "TU_APP_PASSWORD"
  }
}
```

> **Recomendación de seguridad**: Nunca guardes contraseñas ni claves API directamente en `appsettings.json`. Usa User Secrets en desarrollo o variables de entorno en producción.

---

## Base de Datos

### Crear la base de datos

Crea el esquema en MySQL antes de ejecutar la aplicación:

```sql
CREATE DATABASE bibliotecabd CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### Aplicar migraciones

Los scripts SQL se encuentran en `gestion-bibliotecaria/Infrastructure/Persistence/Migrations/`. Aplícalos en orden desde tu cliente MySQL:

```bash
# Ejemplo usando la CLI de MySQL
mysql -u root -p bibliotecabd < gestion-bibliotecaria/Infrastructure/Persistence/Migrations/20260406_step2_auth_auditoria.sql
```

---

## Cómo Ejecutar

```bash
# 1. Clona el repositorio
git clone https://github.com/HenryRiveraM/gestion-bibliotecaria.git
cd gestion-bibliotecaria

# 2. Restaura las dependencias
dotnet restore gestion-bibliotecaria/gestion-bibliotecaria.csproj

# 3. Configura la base de datos (ver sección anterior)

# 4. Ejecuta la aplicación
dotnet run --project gestion-bibliotecaria/gestion-bibliotecaria.csproj
```

La aplicación estará disponible en `https://localhost:5001` (o el puerto configurado en `Properties/launchSettings.json`).

---

## Estructura del Proyecto

```
gestion-bibliotecaria/
├── Aplicacion/
│   ├── Interfaces/          # Contratos de los servicios de aplicación
│   └── Servicios/           # Implementaciones de los servicios
├── Domain/
│   ├── Common/              # Tipos compartidos: Result, Error, EmailMessage
│   ├── Entities/            # Entidades de dominio: Libro, Autor, Ejemplar, Usuario
│   ├── Errors/              # Errores de dominio por módulo
│   ├── Ports/               # Interfaces de repositorios
│   └── Validations/         # Lógica de validación de entradas
├── Infrastructure/
│   ├── Configuration/       # EmailSettings, ConfigurationSingleton
│   ├── Creators/            # Factories de repositorios
│   ├── Email/               # Implementaciones del servicio de correo
│   ├── Persistence/
│   │   ├── Migrations/      # Scripts SQL de migración
│   │   ├── AutorRepository.cs
│   │   ├── EjemplarRepository.cs
│   │   ├── LibroRepository.cs
│   │   └── UsuarioRepository.cs
│   └── Security/            # RouteTokenService (IDs cifrados), SessionKeys
├── Pages/
│   ├── Autores/             # Gestión de autores
│   ├── Ejemplar/            # Gestión de ejemplares
│   ├── Libros/              # Gestión de libros
│   ├── Usuarios/            # Gestión de usuarios (solo Admin)
│   ├── Shared/              # Layout compartido (_Layout.cshtml)
│   ├── Index.cshtml         # Página de inicio
│   └── Login.cshtml         # Autenticación
├── wwwroot/
│   ├── assets/              # Imágenes e íconos
│   ├── css/                 # Hojas de estilo por página
│   └── js/                  # Scripts del cliente
├── appsettings.json
└── Program.cs
```

---

## Roles de Usuario

| Rol            | Descripción                                                                 |
|----------------|-----------------------------------------------------------------------------|
| `Admin`        | Acceso completo: puede gestionar libros, autores, ejemplares **y usuarios** |
| `Bibliotecario`| Acceso a las operaciones de biblioteca: libros, autores y ejemplares        |

La sesión se gestiona mediante cookies HTTP-only con un tiempo de inactividad de 30 minutos. Los IDs de los registros en las rutas se transmiten cifrados mediante `RouteTokenService` (ASP.NET Data Protection) para prevenir enumeración de recursos.

---

## Licencia

Este proyecto es de uso académico. Consulta a los autores para cualquier otro uso.
