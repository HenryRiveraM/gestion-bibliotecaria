using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Security;

namespace gestion_bibliotecaria.Pages;

public class LibroModel : PageModel
{
    private const string QueryLibros = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                                         FROM libro
                                         ORDER BY Titulo ASC";

    private const string QueryAutores = "SELECT AutorId, Nombres, Apellidos FROM autor";

    private readonly IConfiguration _configuration;
    private readonly RouteTokenService _routeTokenService;

    public DataTable Libros { get; set; } = new DataTable();
    public Dictionary<int, string> AutoresNombres { get; set; } = new();

    public LibroModel(IConfiguration configuration, RouteTokenService routeTokenService)
    {
        _configuration = configuration;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        Libros = ObtenerLibros();

        if (!Libros.Columns.Contains("LibroToken"))
        {
            Libros.Columns.Add("LibroToken", typeof(string));
        }

        foreach (DataRow row in Libros.Rows)
        {
            var libroId = row.Field<int>("LibroId");
            row["LibroToken"] = _routeTokenService.CrearToken(libroId);
        }

        AutoresNombres = ObtenerNombresAutores();
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private DataTable ObtenerLibros()
    {
        var dataTable = new DataTable();

        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryLibros, connection))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
        }

        return dataTable;
    }

    private Dictionary<int, string> ObtenerNombresAutores()
    {
        var autores = new Dictionary<int, string>();

        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryAutores, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var nombres = reader.GetString("Nombres");
                        var apellidos = reader.GetString("Apellidos");
                        autores[reader.GetInt32("AutorId")] = $"{nombres} {apellidos}";
                    }
                }
            }
        }

        return autores;
    }
}
