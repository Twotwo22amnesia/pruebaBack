namespace MarcasAutos.Api.Models;

/// <summary>
/// Marca de automóvil registrada en la tabla MarcasAutos.
/// </summary>
public class MarcaAuto
{
    public int Id { get; set; }

    /// <summary>Nombre comercial de la marca (único).</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>País donde se fundó la marca.</summary>
    public string PaisOrigen { get; set; } = string.Empty;

    /// <summary>Año de fundación de la marca.</summary>
    public int AnioFundacion { get; set; }
}
