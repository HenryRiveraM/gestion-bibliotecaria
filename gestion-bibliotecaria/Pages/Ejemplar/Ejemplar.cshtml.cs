using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.FactoryCreators;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class EjemplarModel : PageModel
{
    private readonly IRepository<Ejemplar, int> _repository;
    private readonly RouteTokenService _routeTokenService;
    private readonly IConfiguration _configuration;

    public List<Ejemplar> Ejemplares { get; set; } = new();
    public Dictionary<int, string> LibrosTitulos { get; set; } = new();

    public EjemplarModel(
        RepositoryFactory<Ejemplar, int> factory,
        RouteTokenService routeTokenService,
        IConfiguration configuration)
    {
        _repository = factory.CreateRepository();
        _routeTokenService = routeTokenService;
        _configuration = configuration;
    }

    public async Task OnGetAsync()
    {
        var tabla = _repository.GetAll();

        Ejemplares = new List<Ejemplar>();

        foreach (DataRow row in tabla.Rows)
        {
            var ejemplar = new Ejemplar
            {
                EjemplarId = Convert.ToInt32(row["EjemplarId"]),
                LibroId = Convert.ToInt32(row["LibroId"]),
                CodigoInventario = row["CodigoInventario"].ToString()!,
                EstadoConservacion = row["EstadoConservacion"] == DBNull.Value ? null : row["EstadoConservacion"].ToString(),
                Disponible = Convert.ToBoolean(row["Disponible"]),
                DadoDeBaja = Convert.ToBoolean(row["DadoDeBaja"]),
                MotivoBaja = row["MotivoBaja"] == DBNull.Value ? null : row["MotivoBaja"].ToString(),
                Ubicacion = row["Ubicacion"] == DBNull.Value ? null : row["Ubicacion"].ToString(),
                Estado = Convert.ToBoolean(row["Estado"])
            };

            ejemplar.RouteToken = _routeTokenService.CrearToken(ejemplar.EjemplarId);

            Ejemplares.Add(ejemplar);
        }

        LibrosTitulos = await ObtenerTitulosLibrosAsync();
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