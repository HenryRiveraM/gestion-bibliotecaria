using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Data;
using gestion_bibliotecaria.Security;

namespace gestion_bibliotecaria.Pages;

public class AutorModel : PageModel
{
    public DataTable AutorDataTable { get; set; } = new DataTable();

    private readonly IConfiguration configuration;
    private readonly RouteTokenService _routeTokenService;

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Orden { get; set; }

    public AutorModel(IConfiguration configuration, RouteTokenService routeTokenService)
    {
        this.configuration = configuration;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        Select();
    }

    void Select()
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        string orderBy = "Apellidos ASC";

        if (Orden == "nombre")
            orderBy = "Nombres ASC";

        string query = $@"
        SELECT AutorId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado
        FROM autor
        WHERE (@buscar IS NULL
            OR Nombres LIKE CONCAT('%', @buscar, '%')
            OR Apellidos LIKE CONCAT('%', @buscar, '%')
            OR Nacionalidad LIKE CONCAT('%', @buscar, '%'))
        ORDER BY {orderBy}";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@buscar", Buscar);

            MySqlDataAdapter adapter = new MySqlDataAdapter(command);

            adapter.Fill(AutorDataTable);

            if (!AutorDataTable.Columns.Contains("AutorToken"))
            {
                AutorDataTable.Columns.Add("AutorToken", typeof(string));
            }

            foreach (DataRow row in AutorDataTable.Rows)
            {
                var autorId = Convert.ToInt32(row["AutorId"]);
                row["AutorToken"] = _routeTokenService.CrearToken(autorId);
            }
        }
    }
}