using MarcasAutos.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// La cadena de conexión se lee de configuración: appsettings.Development.json en local
// y la variable de entorno ConnectionStrings__DefaultConnection dentro de Docker.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        // Reintentos ante fallos transitorios (p. ej. si Postgres aún está arrancando).
        npgsql.EnableRetryOnFailure(maxRetryCount: 5)));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Aplica las migraciones pendientes al iniciar (incluye la creación de la tabla y el seed).
// Así basta con "docker compose up" para tener la base lista. En un entorno productivo
// preferiría ejecutarlas como paso aparte del despliegue.
// Solo aplica a bases relacionales: las pruebas usan InMemory, que no tiene migraciones.
if (app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
