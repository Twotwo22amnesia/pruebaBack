namespace MarcasAutos.Api.Dtos;

/// <summary>
/// Representación de una marca que expone la API.
/// Se separa de la entidad para que cambios en el modelo de datos no rompan
/// el contrato público del endpoint.
/// </summary>
public record MarcaAutoDto(int Id, string Nombre, string PaisOrigen, int AnioFundacion);
