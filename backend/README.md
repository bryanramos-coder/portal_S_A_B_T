# Backend - Portal de Seguimiento a Graduados y Bolsa de Trabajo

API REST desarrollada en C# con ASP.NET Core 8 y PostgreSQL.

## Requisitos

- .NET 8 SDK
- PostgreSQL 17 (o el `Docker-compose.yml` del proyecto de base de datos)

## Configuracion local

1. Levantar PostgreSQL y ejecutar el script de la base de datos (`BASE DE DATOS PARA EL PORTAL.sql`), los catalogos de `sector` y `area_especializacion`, y el usuario administrador inicial.
2. Configurar los secretos de desarrollo desde `backend/PortalGraduados.Api`:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5434;Database=portal_graduados_db;Username=portal_admin;Password=TU_PASSWORD"
dotnet user-secrets set "Jwt:Key" "UNA_CLAVE_SECRETA_DE_MINIMO_32_CARACTERES"
```

Tambien puedes copiar `appsettings.Development.json.example` a `appsettings.Development.json` (ignorado por Git) y colocar tus credenciales locales.

3. Ejecutar la API:

```bash
cd backend/PortalGraduados.Api
dotnet run
```

La documentacion interactiva estara disponible en `http://localhost:5269/swagger`.
