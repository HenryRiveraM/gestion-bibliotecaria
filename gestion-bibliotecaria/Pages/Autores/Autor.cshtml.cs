using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Security;

namespace gestion_bibliotecaria.Pages;

public class AutorModel : PageModel
{
    private const string QueryAutores = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro, UltimaActualizacion
                                          FROM autor
                                          ORDER BY Nombres ASC";

    private readonly IConfiguration _configuration;
    private readonly RouteTokenService _routeTokenService;

    public DataTable AutorDataTable { get; set; } = new DataTable();

    public AutorModel(IConfiguration configuration, RouteTokenService routeTokenService)
    {
        _configuration = configuration;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        AutorDataTable = ObtenerAutores();
        AgregarTokensAAutores();
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private DataTable ObtenerAutores()
    {
        var dataTable = new DataTable();

        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryAutores, connection))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
        }

        return dataTable;
    }

    private void AgregarTokensAAutores()
    {
        if (!AutorDataTable.Columns.Contains("AutorToken"))
        {
            AutorDataTable.Columns.Add("AutorToken", typeof(string));
        }

        foreach (DataRow row in AutorDataTable.Rows)
        {
            var autorId = row.Field<int>("AutorId");
            row["AutorToken"] = _routeTokenService.CrearToken(autorId);
        }
    }
}