using MarcasAutos.Api.Dtos;
using MarcasAutos.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarcasAutos.Api.Controllers;

/// <summary>
/// Endpoints de consulta de marcas de autos.
/// El controlador solo se ocupa de HTTP; el acceso a datos queda en el servicio.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MarcasAutosController(IMarcasAutosService marcasAutosService) : ControllerBase
{
    /// <summary>
    /// Obtiene todas las marcas de autos ordenadas por nombre.
    /// </summary>
    /// <response code="200">Listado de marcas (vacío si no hay registros).</response>
    [HttpGet]
    [ProducesResponseType<IEnumerable<MarcaAutoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MarcaAutoDto>>> GetAll(CancellationToken cancellationToken)
    {
        var marcas = await marcasAutosService.ObtenerTodasAsync(cancellationToken);
        return Ok(marcas);
    }
}
