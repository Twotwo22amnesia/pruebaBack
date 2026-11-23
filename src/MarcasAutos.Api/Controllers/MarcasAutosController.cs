using MarcasAutos.Api.Data;
using MarcasAutos.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Api.Controllers;

/// <summary>
/// Endpoints de consulta de marcas de autos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MarcasAutosController(AppDbContext context) : ControllerBase
{
    /// <summary>
    /// Obtiene todas las marcas de autos ordenadas por nombre.
    /// </summary>
    /// <response code="200">Listado de marcas (vacío si no hay registros).</response>
    [HttpGet]
    [ProducesResponseType<IEnumerable<MarcaAutoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MarcaAutoDto>>> GetAll(CancellationToken cancellationToken)
    {
        // AsNoTracking: es solo lectura, no hace falta que EF siga los cambios de estas entidades.
        // La proyección a DTO se traduce a SQL, así solo se consultan las columnas necesarias.
        var marcas = await context.MarcasAutos
            .AsNoTracking()
            .OrderBy(m => m.Nombre)
            .Select(m => new MarcaAutoDto(m.Id, m.Nombre, m.PaisOrigen, m.AnioFundacion))
            .ToListAsync(cancellationToken);

        return Ok(marcas);
    }
}
