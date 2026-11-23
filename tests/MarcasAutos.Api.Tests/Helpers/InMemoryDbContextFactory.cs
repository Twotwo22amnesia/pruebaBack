using MarcasAutos.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Api.Tests.Helpers;

/// <summary>
/// Crea instancias de <see cref="AppDbContext"/> sobre el proveedor InMemory de EF Core.
/// </summary>
internal static class InMemoryDbContextFactory
{
    /// <param name="conSeed">
    /// Si es true se llama a EnsureCreated, que en InMemory aplica los datos de HasData
    /// (los mismos que inserta la migración en PostgreSQL).
    /// </param>
    public static AppDbContext Crear(bool conSeed = true)
    {
        // Un nombre de base distinto por contexto evita que las pruebas compartan datos
        // entre sí, ya que InMemory mantiene las bases vivas durante todo el proceso.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"MarcasAutosTests_{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);

        if (conSeed)
        {
            context.Database.EnsureCreated();
        }

        return context;
    }
}
