using System.Net;
using System.Net.Http.Json;
using MarcasAutos.Api.Data;
using MarcasAutos.Api.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MarcasAutos.Api.Tests.Integration;

/// <summary>
/// Levanta la API completa en memoria (sin red ni Docker) reemplazando PostgreSQL por
/// el proveedor InMemory. Complementa las pruebas unitarias comprobando el pipeline
/// real: ruteo, serialización JSON e inyección de dependencias.
/// </summary>
public class MarcasAutosApiTests(MarcasAutosApiTests.ApiFactory factory)
    : IClassFixture<MarcasAutosApiTests.ApiFactory>
{
    [Fact]
    public async Task Get_MarcasAutos_Retorna200ConLasMarcasEnJson()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/marcasautos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var marcas = await response.Content.ReadFromJsonAsync<List<MarcaAutoDto>>();
        Assert.NotNull(marcas);
        Assert.Equal(MarcasAutosSeed.Marcas.Count, marcas.Count);
    }

    [Fact]
    public async Task Get_DocumentoOpenApi_EstaDisponibleEnDevelopment()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/MarcasAutos", await response.Content.ReadAsStringAsync());
    }

    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _nombreBase = $"MarcasAutosApi_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            // Program.cs exige una cadena de conexión; aquí no se usa porque se cambia el proveedor.
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=no-usado");

            builder.ConfigureServices(services =>
            {
                // Se quita la configuración de Npgsql registrada en Program.cs antes de
                // registrar InMemory; EF no permite dos proveedores en el mismo contexto.
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_nombreBase));

                using var scope = services.BuildServiceProvider().CreateScope();
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
            });
        }
    }
}
