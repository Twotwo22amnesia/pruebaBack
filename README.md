# API Marcas de Autos

API REST en ASP.NET Core (.NET 10) que expone las marcas de autos guardadas en PostgreSQL
mediante Entity Framework Core. Incluye migración con datos iniciales, pruebas con xUnit
y un entorno con Docker Compose.

## Tecnologías

- .NET 10 / ASP.NET Core (controladores)
- Entity Framework Core 10 + Npgsql (PostgreSQL)
- xUnit, EF Core InMemory, NSubstitute, coverlet y ReportGenerator
- Docker y Docker Compose (PostgreSQL 17)

## Estructura

```
├─ src/MarcasAutos.Api
│  ├─ Controllers/MarcasAutosController.cs   Endpoint GET /api/marcasautos
│  ├─ Data/AppDbContext.cs                   DbContext y configuración de la tabla
│  ├─ Data/MarcasAutosSeed.cs                Datos iniciales (HasData)
│  ├─ Data/Migrations/                       Migración InitialCreate
│  ├─ Dtos/MarcaAutoDto.cs                   Contrato de respuesta
│  ├─ Models/MarcaAuto.cs                    Entidad
│  ├─ Services/                              Lógica de consulta (IMarcasAutosService)
│  └─ Program.cs                             Configuración y arranque
├─ tests/MarcasAutos.Api.Tests
│  ├─ Controllers/                           Pruebas unitarias del controlador
│  ├─ Services/                              Pruebas del servicio con contexto en memoria
│  ├─ Data/                                  Pruebas del modelo y del seed
│  ├─ Integration/                           API completa con WebApplicationFactory
│  └─ Helpers/                               Fábrica del contexto en memoria
├─ Dockerfile
├─ docker-compose.yml
└─ coverage.runsettings
```

## Ejecución con Docker Compose

Requisito: Docker Desktop (o Docker Engine con el plugin compose).

```bash
docker compose up --build
```

Esto levanta dos servicios:

| Servicio | Descripción | Puerto |
|----------|-------------|--------|
| `db`     | PostgreSQL 17 con volumen persistente y healthcheck | 5432 |
| `api`    | API REST; espera a que `db` esté saludable antes de iniciar | 8080 |

Al arrancar, la API aplica las migraciones pendientes, lo que crea la tabla `MarcasAutos`
y carga las marcas iniciales. Luego:

- Endpoint: <http://localhost:8080/api/marcasautos>
- Documentación interactiva: <http://localhost:8080/scalar>
- Documento OpenAPI: <http://localhost:8080/openapi/v1.json>

Ejemplo de respuesta:

```json
[
  { "id": 3, "nombre": "Chevrolet", "paisOrigen": "Estados Unidos", "anioFundacion": 1911 },
  { "id": 2, "nombre": "Ford", "paisOrigen": "Estados Unidos", "anioFundacion": 1903 },
  ...
]
```

Las credenciales tienen valores por defecto de desarrollo. Para cambiarlas, copiar
`.env.example` como `.env` y editarlo.

Para detener y borrar también los datos: `docker compose down -v`.

## Ejecución local (sin Docker para la API)

Requisitos: SDK de .NET 10 y un PostgreSQL accesible. La cadena de conexión de desarrollo
está en `src/MarcasAutos.Api/appsettings.Development.json`
(`localhost:5432`, base `marcasautos`, usuario/clave `postgres`). Se puede levantar
solo la base con `docker compose up db`.

```bash
dotnet run --project src/MarcasAutos.Api
```

La API queda en <http://localhost:5026>.

### Migraciones

La herramienta `dotnet-ef` está declarada como herramienta local:

```bash
dotnet tool restore
dotnet ef migrations add <Nombre> --project src/MarcasAutos.Api --output-dir Data/Migrations
dotnet ef database update --project src/MarcasAutos.Api
```

## Pruebas

```bash
dotnet test
```

Las pruebas no necesitan PostgreSQL: usan el proveedor InMemory de EF Core, con una base
distinta por prueba para que no compartan estado.

- **Servicio** (contexto en memoria): todas las marcas, orden por nombre, mapeo de campos
  al DTO, lista vacía sin registros y respeto de la cancelación.
- **Controlador**: con el servicio real sobre InMemory verifica que el endpoint devuelve las
  marcas del seed; con el servicio simulado (NSubstitute) verifica de forma aislada que
  responde 200 con lo que entrega el servicio y que propaga el `CancellationToken`.
- **Modelo y seed**: al menos tres marcas, sin Id ni nombres repetidos, tabla `MarcasAutos`
  e índice único en `Nombre`.
- **Integración**: levanta la API con `WebApplicationFactory` cambiando PostgreSQL por
  InMemory y consulta el endpoint por HTTP.

También se pueden ejecutar dentro de un contenedor:

```bash
docker compose --profile tests run --rm tests
```

### Cobertura

```bash
dotnet test --collect "XPlat Code Coverage" --results-directory TestResults
dotnet tool restore
dotnet reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:"Html;TextSummary"
```

El reporte queda en `coverage-report/index.html`. Cobertura actual: **95,9 % de líneas**
y 80 % de ramas (objetivo mínimo: 70 %).

Las migraciones se excluyen del cálculo porque las genera `dotnet ef` y no contienen
lógica propia. La exclusión está en `coverage.runsettings`, que el proyecto de pruebas
aplica automáticamente (`RunSettingsFilePath`), por lo que no hace falta indicarlo al
ejecutar `dotnet test`.

## Decisiones de diseño

- **Capas**: `Controller → IMarcasAutosService → AppDbContext`. El controlador solo maneja
  HTTP y depende de una interfaz; la consulta y el mapeo viven en el servicio.
- **Sin repositorio propio**: `DbContext` ya implementa los patrones Repository (`DbSet`) y
  Unit of Work; envolverlo en otro repositorio duplicaría su API sin beneficio en este alcance.
- **Seed con `HasData`**: los datos quedan dentro de la migración, así crear la tabla y
  cargarla es un único paso versionado.
- **Secuencia de identidad desde 6**: como el seed inserta Id explícitos (1 a 5), la
  columna identity arranca después para que los siguientes INSERT no choquen.
- **Migraciones al iniciar**: simplifica levantar el entorno con un solo comando. Se puede
  desactivar con `Database__ApplyMigrationsOnStartup=false`; en producción conviene
  aplicarlas como paso del despliegue.
- **DTO de respuesta**: separa el contrato público de la entidad de base de datos.
- **Consultas de solo lectura** con `AsNoTracking` y proyección directa al DTO.
- **Reintentos de conexión** (`EnableRetryOnFailure`) ante fallos transitorios de red.
