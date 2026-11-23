using MarcasAutos.Api.Data;
using MarcasAutos.Api.Dtos;
using MarcasAutos.Api.Models;
using MarcasAutos.Api.Services;
using MarcasAutos.Api.Tests.Helpers;

namespace MarcasAutos.Api.Tests.Services;

/// <summary>
/// Pruebas del servicio contra un contexto en memoria: aquí se valida la consulta
/// (datos, orden y mapeo al DTO).
/// </summary>
public class MarcasAutosServiceTests
{
    [Fact]
    public async Task ObtenerTodasAsync_ConDatosIniciales_RetornaTodasLasMarcas()
    {
        await using var context = InMemoryDbContextFactory.Crear();
        var service = new MarcasAutosService(context);

        var marcas = await service.ObtenerTodasAsync();

        Assert.Equal(MarcasAutosSeed.Marcas.Count, marcas.Count);
        Assert.Equal(
            MarcasAutosSeed.Marcas.Select(m => m.Nombre).Order(),
            marcas.Select(m => m.Nombre).Order());
    }

    [Fact]
    public async Task ObtenerTodasAsync_RetornaMarcasOrdenadasPorNombre()
    {
        await using var context = InMemoryDbContextFactory.Crear();
        var service = new MarcasAutosService(context);

        var marcas = await service.ObtenerTodasAsync();

        Assert.Equal(marcas.Select(m => m.Nombre).Order(), marcas.Select(m => m.Nombre));
    }

    [Fact]
    public async Task ObtenerTodasAsync_MapeaTodosLosCamposDeLaEntidad()
    {
        await using var context = InMemoryDbContextFactory.Crear(conSeed: false);
        context.MarcasAutos.Add(new MarcaAuto
        {
            Id = 10,
            Nombre = "Mazda",
            PaisOrigen = "Japón",
            AnioFundacion = 1920
        });
        await context.SaveChangesAsync();
        var service = new MarcasAutosService(context);

        var marcas = await service.ObtenerTodasAsync();

        var marca = Assert.Single(marcas);
        Assert.Equal(new MarcaAutoDto(10, "Mazda", "Japón", 1920), marca);
    }

    [Fact]
    public async Task ObtenerTodasAsync_SinRegistros_RetornaListaVacia()
    {
        await using var context = InMemoryDbContextFactory.Crear(conSeed: false);
        var service = new MarcasAutosService(context);

        var marcas = await service.ObtenerTodasAsync();

        Assert.Empty(marcas);
    }

    [Fact]
    public async Task ObtenerTodasAsync_ConCancelacionSolicitada_LanzaOperationCanceledException()
    {
        await using var context = InMemoryDbContextFactory.Crear();
        var service = new MarcasAutosService(context);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ObtenerTodasAsync(cts.Token));
    }
}
