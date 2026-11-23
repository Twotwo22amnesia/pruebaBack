using MarcasAutos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Api.Data;

/// <summary>
/// Contexto de EF Core de la aplicación.
/// El proveedor (PostgreSQL en ejecución, InMemory en pruebas) se decide desde
/// fuera mediante <see cref="DbContextOptions{TContext}"/>, así el contexto no
/// queda acoplado a ninguna base de datos concreta.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MarcaAuto> MarcasAutos => Set<MarcaAuto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MarcaAuto>(entity =>
        {
            // Nombre de tabla explícito según lo solicitado en el enunciado.
            entity.ToTable("MarcasAutos");

            entity.HasKey(m => m.Id);

            entity.Property(m => m.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            // No tiene sentido registrar dos veces la misma marca.
            entity.HasIndex(m => m.Nombre).IsUnique();

            entity.Property(m => m.PaisOrigen)
                .IsRequired()
                .HasMaxLength(60);
        });
    }
}
