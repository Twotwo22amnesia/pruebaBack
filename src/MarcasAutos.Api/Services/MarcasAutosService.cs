using MarcasAutos.Api.Data;
using MarcasAutos.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Api.Services;

/// <summary>
/// Implementación de <see cref="IMarcasAutosService"/> sobre EF Core.
/// No se agrega un repositorio intermedio: el DbContext ya cumple ese papel
/// (DbSet como repositorio y el contexto como unidad de trabajo), y otra capa
/// encima solo duplicaría su API sin aportar nada en este alcance.
/// </summary>
public class MarcasAutosService(AppDbContext context) : IMarcasAutosService
{
    public async Task<IReadOnlyList<MarcaAutoDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        // AsNoTracking: es solo lectura, no hace falta que EF siga los cambios de estas entidades.
        // La proyección a DTO se traduce a SQL, así solo se consultan las columnas necesarias.
        return await context.MarcasAutos
            .AsNoTracking()
            .OrderBy(m => m.Nombre)
            .Select(m => new MarcaAutoDto(m.Id, m.Nombre, m.PaisOrigen, m.AnioFundacion))
            .ToListAsync(cancellationToken);
    }
}
