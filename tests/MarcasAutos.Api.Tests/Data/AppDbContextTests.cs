using MarcasAutos.Api.Data;
using MarcasAutos.Api.Models;
using MarcasAutos.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Api.Tests.Data;

/// <summary>
/// Verifica la configuración del modelo y los datos iniciales.
/// </summary>
public class AppDbContextTests
{
    [Fact]
    public void Seed_ContieneAlMenosTresMarcas()
    {
        // Requisito mínimo del enunciado.
        Assert.True(MarcasAutosSeed.Marcas.Count >= 3);
    }

    [Fact]
    public void Seed_NoTieneIdsNiNombresRepetidos()
    {
        Assert.Equal(MarcasAutosSeed.Marcas.Count, MarcasAutosSeed.Marcas.Select(m => m.Id).Distinct().Count());
        Assert.Equal(MarcasAutosSeed.Marcas.Count, MarcasAutosSeed.Marcas.Select(m => m.Nombre).Distinct().Count());
    }

    [Fact]
    public void EnsureCreated_CargaLosDatosIniciales()
    {
        using var context = InMemoryDbContextFactory.Crear();

        var nombres = context.MarcasAutos.Select(m => m.Nombre).ToList();

        Assert.Equal(MarcasAutosSeed.Marcas.Select(m => m.Nombre).Order(), nombres.Order());
    }

    [Fact]
    public void Modelo_MapeaLaEntidadALaTablaMarcasAutos()
    {
        using var context = InMemoryDbContextFactory.Crear(conSeed: false);

        var entidad = context.Model.FindEntityType(typeof(MarcaAuto));

        Assert.NotNull(entidad);
        Assert.Equal("MarcasAutos", entidad.GetTableName());
        Assert.Equal(100, entidad.FindProperty(nameof(MarcaAuto.Nombre))!.GetMaxLength());
        Assert.Contains(entidad.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == nameof(MarcaAuto.Nombre));
    }
}
