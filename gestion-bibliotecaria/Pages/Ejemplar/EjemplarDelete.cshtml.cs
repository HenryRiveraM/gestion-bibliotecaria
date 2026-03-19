using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Pages;

public class EjemplarDeleteModel : PageModel
{
    private readonly IRepository<Ejemplar, int> _repository;
    private readonly RouteTokenService _routeTokenService;
    private readonly IConfiguration _configuration;

    public Ejemplar Ejemplar { get; set; } = new Ejemplar();
    public string TituloLibro { get; set; } = string.Empty;

    [BindProperty]
    public string EjemplarToken { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarDeleteModel(
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

        EjemplarToken = token;

        var ejemplar = _repository.GetById(id);

        if (ejemplar == null)
        {
            return NotFound();
        }

        Ejemplar = ejemplar;
        TituloLibro = await ObtenerTituloLibroAsync(ejemplar.LibroId);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_routeTokenService.TryObtenerId(EjemplarToken, out var id))
        {
            return NotFound();
        }

        try
        {
            var ejemplar = _repository.GetById(id);

            if (ejemplar == null)
            {
                return NotFound();
            }

            _repository.Delete(ejemplar);

            return Redirect("/Ejemplar");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al eliminar el ejemplar: {ex.Message}";

            var ejemplar = _repository.GetById(id);
            if (ejemplar != null)
            {
                Ejemplar = ejemplar;
                TituloLibro = await ObtenerTituloLibroAsync(ejemplar.LibroId);
            }

            return Page();
        }
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string not found.");

    private async Task<string> ObtenerTituloLibroAsync(int libroId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        string query = "SELECT Titulo FROM libro WHERE LibroId = @LibroId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@LibroId", libroId);

        var result = await command.ExecuteScalarAsync();
        return result?.ToString() ?? "Libro no encontrado";
    }
}