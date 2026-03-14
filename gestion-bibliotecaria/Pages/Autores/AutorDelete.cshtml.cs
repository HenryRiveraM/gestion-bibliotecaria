using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;

namespace gestion_bibliotecaria.Pages;

public class AutorDeleteModel : PageModel
{
    private readonly IConfiguration configuration;
    private readonly RouteTokenService _routeTokenService;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor();

    [BindProperty]
    public string AutorToken { get; set; } = string.Empty;

    public AutorDeleteModel(IConfiguration configuration, RouteTokenService routeTokenService)
    {
        this.configuration = configuration;
        _routeTokenService = routeTokenService;
    }

    public IActionResult OnGet(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        AutorToken = token;

        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        string query = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad
                         FROM autor
                         WHERE AutorId=@id";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Autor.AutorId = reader.GetInt32("AutorId");
                    Autor.Nombres = reader.GetString("Nombres");
                    Autor.Apellidos = reader.GetString("Apellidos");
                    Autor.Nacionalidad = reader["Nacionalidad"]?.ToString();
                }
            }
        }

        if (Autor.AutorId == 0)
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!_routeTokenService.TryObtenerId(AutorToken, out var autorId))
        {
            return NotFound();
        }

        Autor.AutorId = autorId;

        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        string query = @"UPDATE autor
                         SET
                            Estado= 0,
                            UltimaActualizacion=NOW()
                         WHERE AutorId=@AutorId";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@AutorId", Autor.AutorId);

            command.ExecuteNonQuery();
        }

        return RedirectToPage("Autor");
    }
}