using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class AutorDeleteModel : PageModel
{
    private readonly IConfiguration configuration;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor();

    public AutorDeleteModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void OnGet(int id)
    {
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
    }

    public IActionResult OnPost()
    {
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