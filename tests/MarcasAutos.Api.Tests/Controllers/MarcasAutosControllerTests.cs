using MarcasAutos.Api.Controllers;
using MarcasAutos.Api.Data;
using MarcasAutos.Api.Dtos;
using MarcasAutos.Api.Services;
using MarcasAutos.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MarcasAutos.Api.Tests.Controllers;

/// <summary>
/// Pruebas unitarias del controlador. Se instancia directamente, sin levantar el servidor HTTP.
/// </summary>
public class MarcasAutosControllerTests
{
    [Fact]
    public async Task GetAll_ConContextoEnMemoria_RetornaOkConLasMarcasDelSeed()
    {
        // Controlador + servicio real sobre la base en memoria: verifica que el endpoint
        // devuelve los datos esperados de punta a punta, sin PostgreSQL.
        await using var context = InMemoryDbContextFactory.Crear();
        var controller = new MarcasAutosController(new MarcasAutosService(context));

        var resultado = await controller.GetAll(CancellationToken.None);

        var marcas = ObtenerMarcas(resultado);
        Assert.Equal(
            MarcasAutosSeed.Marcas.Select(m => m.Nombre).Order(),
            marcas.Select(m => m.Nombre));
    }

    [Fact]
    public async Task GetAll_RetornaOkConLoQueEntregaElServicio()
    {
        // Con el servicio simulado se prueba solo la responsabilidad del controlador:
        // envolver el resultado en un 200 sin alterarlo.
        IReadOnlyList<MarcaAutoDto> esperadas =
        [
            new(1, "Toyota", "Japón", 1937),
            new(2, "Ford", "Estados Unidos", 1903),
        ];
        var service = Substitute.For<IMarcasAutosService>();
        service.ObtenerTodasAsync(Arg.Any<CancellationToken>()).Returns(esperadas);
        var controller = new MarcasAutosController(service);

        var resultado = await controller.GetAll(CancellationToken.None);

        Assert.Equal(esperadas, ObtenerMarcas(resultado));
    }

    [Fact]
    public async Task GetAll_SinMarcas_RetornaOkConListaVacia()
    {
        var service = Substitute.For<IMarcasAutosService>();
        service.ObtenerTodasAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<MarcaAutoDto>());
        var controller = new MarcasAutosController(service);

        var resultado = await controller.GetAll(CancellationToken.None);

        // Sin datos se espera 200 con lista vacía, no 404: la colección existe aunque esté vacía.
        Assert.Empty(ObtenerMarcas(resultado));
    }

    [Fact]
    public async Task GetAll_PropagaElCancellationTokenAlServicio()
    {
        var service = Substitute.For<IMarcasAutosService>();
        service.ObtenerTodasAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<MarcaAutoDto>());
        var controller = new MarcasAutosController(service);
        using var cts = new CancellationTokenSource();

        await controller.GetAll(cts.Token);

        // Si el cliente cancela la petición, la consulta a la base también debe cancelarse.
        await service.Received(1).ObtenerTodasAsync(cts.Token);
    }

    private static List<MarcaAutoDto> ObtenerMarcas(ActionResult<IEnumerable<MarcaAutoDto>> resultado)
    {
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        return Assert.IsAssignableFrom<IEnumerable<MarcaAutoDto>>(ok.Value).ToList();
    }
}
