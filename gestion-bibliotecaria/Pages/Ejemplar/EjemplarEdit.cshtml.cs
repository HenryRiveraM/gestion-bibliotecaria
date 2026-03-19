using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.Validaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class EjemplarEditModel : PageModel
{
    private readonly IRepository<Ejemplar, int> _repository;
    private readonly RouteTokenService _routeTokenService;
    private readonly IConfiguration _configuration;

    [BindProperty]
    public Ejemplar Ejemplar { get; set; } = new Ejemplar();

    [BindProperty]
    public string EjemplarToken { get; set; } = string.Empty;

    public Dictionary<int, string> LibrosTitulos { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarEditModel(
        RepositoryFactory<Ejemplar, int> factory,
        RouteTokenService routeTokenService,
        IConfiguration configuration)
    {
        _repository = factory.CreateRepository();
        _routeTokenService = routeTokenService;
        _configuration = configuration;
    }


    public async Task<IActionResult> OnGetAsync(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        var ejemplar = _repository.GetById(id);

        if (ejemplar == null)
        {
            return NotFound();
        }

        Ejemplar = ejemplar;
        EjemplarToken = token;

        LibrosTitulos = await ObtenerTitulosLibrosAsync();

        return Page();
    }


    public IActionResult OnPost()
    {
        if (!_routeTokenService.TryObtenerId(EjemplarToken, out var ejemplarId))
        {
            return NotFound();
        }


        Ejemplar.CodigoInventario = ValidadorEntrada.NormalizarEspacios(Ejemplar.CodigoInventario);
        Ejemplar.EstadoConservacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.EstadoConservacion);
        Ejemplar.Ubicacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.Ubicacion);
        Ejemplar.MotivoBaja = ValidadorEntrada.NormalizarEspacios(Ejemplar.MotivoBaja);


        if (ValidadorEntrada.EstaVacio(Ejemplar.CodigoInventario))
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "El código de inventario es obligatorio.");
        }
        else if (!ValidadorEntrada.CodigoInventarioValido(Ejemplar.CodigoInventario))
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "El código de inventario solo puede tener letras, números o guiones.");
        }
        else if (ValidadorEntrada.ExcedeLongitud(Ejemplar.CodigoInventario, 30))
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "El código de inventario excede la longitud máxima de 30 caracteres.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }


        var ejemplar = new Ejemplar
        {
            EjemplarId = ejemplarId,
            LibroId = Ejemplar.LibroId,
            CodigoInventario = Ejemplar.CodigoInventario,
            EstadoConservacion = Ejemplar.EstadoConservacion,
            Disponible = Ejemplar.Disponible,
            DadoDeBaja = Ejemplar.DadoDeBaja,
            MotivoBaja = Ejemplar.MotivoBaja,
            Ubicacion = Ejemplar.Ubicacion,
            Estado = Ejemplar.Estado
        };

        try
        {
            _repository.Update(ejemplar);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "Ya existe un ejemplar con ese código de inventario.");
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el ejemplar. Por favor, intentá nuevamente.");
            return Page();
        }

        return Redirect("/Ejemplar");
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string not found.");

    private async Task<Dictionary<int, string>> ObtenerTitulosLibrosAsync()
    {
        var titulos = new Dictionary<int, string>();

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        string query = "SELECT LibroId, Titulo FROM libro";

        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            titulos[reader.GetInt32("LibroId")] = reader.GetString("Titulo");
        }

        return titulos;
    }
}