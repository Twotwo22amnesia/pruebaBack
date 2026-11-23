using MarcasAutos.Api.Dtos;

namespace MarcasAutos.Api.Services;

/// <summary>
/// Operaciones de consulta sobre las marcas de autos.
/// El controlador depende de esta interfaz y no de EF Core, lo que permite
/// cambiar la implementación o reemplazarla por un doble en las pruebas.
/// </summary>
public interface IMarcasAutosService
{
    /// <summary>Obtiene todas las marcas ordenadas por nombre.</summary>
    Task<IReadOnlyList<MarcaAutoDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
}
