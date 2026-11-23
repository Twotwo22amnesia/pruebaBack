using MarcasAutos.Api.Controllers;
using MarcasAutos.Api.Data;
using MarcasAutos.Api.Dtos;
using MarcasAutos.Api.Models;
using MarcasAutos.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace MarcasAutos.Api.Tests.Controllers;

/// <summary>
/// Pruebas unitarias del controlador usando un contexto en memoria.
/// Se instancia el controlador directamente, sin levantar el servidor HTTP.
/// </summary>
public class MarcasAutosControllerTests
{
    [Fact]
    public async Task GetAll_ConDatosIniciales_RetornaOkConTodasLasMarcas()
    {
        await using var context = InMemoryDbContextFactory.Crear();
        var controller = new MarcasAutosController(context);

        var resultado = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var marcas = Assert.IsAssignableFrom<IEnumerable<MarcaAutoDto>>(ok.Value).ToList();
        Assert.Equal(MarcasAutosSeed.Marcas.Count, marcas.Count);
        Assert.Equal(
            MarcasAutosSeed.Marcas.Select(m => m.Nombre).Order(),
            marcas.Select(m => m.Nombre).Order());
    }

    [Fact]
    public async Task GetAll_RetornaMarcasOrdenadasPorNombre()
    {
        await using var context = InMemoryDbContextFactory.Crear();
        var controller = new MarcasAutosController(context);

        var resultado = await controller.GetAll(CancellationToken.None);

        var marcas = ObtenerMarcas(resultado);
        Assert.Equal(marcas.Select(m => m.Nombre).Order(), marcas.Select(m => m.Nombre));
    }

    [Fact]
    public async Task GetAll_MapeaTodosLosCamposDeLaEntidad()
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
        var controller = new MarcasAutosController(context);

        var resultado = await controller.GetAll(CancellationToken.None);

        var marca = Assert.Single(ObtenerMarcas(resultado));
        Assert.Equal(new MarcaAutoDto(10, "Mazda", "Japón", 1920), marca);
    }

    [Fact]
    public async Task GetAll_SinRegistros_RetornaOkConListaVacia()
    {
        await using var context = InMemoryDbContextFactory.Crear(conSeed: false);
        var controller = new MarcasAutosController(context);

        var resultado = await controller.GetAll(CancellationToken.None);

        // Sin datos se espera 200 con lista vacía, no 404: la colección existe aunque esté vacía.
        Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Empty(ObtenerMarcas(resultado));
    }

    [Fact]
    public async Task GetAll_ConCancelacionSolicitada_LanzaOperationCanceledException()
    {
        await using var context = InMemoryDbContextFactory.Crear();
        var controller = new MarcasAutosController(context);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => controller.GetAll(cts.Token));
    }

    private static List<MarcaAutoDto> ObtenerMarcas(ActionResult<IEnumerable<MarcaAutoDto>> resultado)
    {
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        return Assert.IsAssignableFrom<IEnumerable<MarcaAutoDto>>(ok.Value).ToList();
    }
}
