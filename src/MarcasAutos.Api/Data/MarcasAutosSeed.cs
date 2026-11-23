using MarcasAutos.Api.Models;

namespace MarcasAutos.Api.Data;

/// <summary>
/// Datos iniciales de la tabla MarcasAutos.
/// Se cargan con HasData, por lo que forman parte de la migración: al aplicar
/// las migraciones la tabla se crea y se llena en el mismo paso.
/// </summary>
public static class MarcasAutosSeed
{
    // Con HasData los Id deben ser fijos; EF los usa para detectar en futuras
    // migraciones si un registro se agregó, se modificó o se eliminó.
    public static readonly IReadOnlyList<MarcaAuto> Marcas =
    [
        new() { Id = 1, Nombre = "Toyota",     PaisOrigen = "Japón",          AnioFundacion = 1937 },
        new() { Id = 2, Nombre = "Ford",       PaisOrigen = "Estados Unidos", AnioFundacion = 1903 },
        new() { Id = 3, Nombre = "Chevrolet",  PaisOrigen = "Estados Unidos", AnioFundacion = 1911 },
        new() { Id = 4, Nombre = "Volkswagen", PaisOrigen = "Alemania",       AnioFundacion = 1937 },
        new() { Id = 5, Nombre = "Hyundai",    PaisOrigen = "Corea del Sur",  AnioFundacion = 1967 },
    ];
}
