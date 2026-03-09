using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class AutorCreateModel : PageModel
{
    private readonly IConfiguration configuration;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor();

    public AutorCreateModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost()
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        string query = @"INSERT INTO autor
                        (Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro)
                        VALUES
                        (@Nombres, @Apellidos, @Nacionalidad, @FechaNacimiento, @Estado, NOW())";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@Nombres", Autor.Nombres);
            command.Parameters.AddWithValue("@Apellidos", Autor.Apellidos);
            command.Parameters.AddWithValue("@Nacionalidad", Autor.Nacionalidad);
            command.Parameters.AddWithValue("@FechaNacimiento", Autor.FechaNacimiento);
            command.Parameters.AddWithValue("@Estado", Autor.Estado);

            command.ExecuteNonQuery();
        }

        return RedirectToPage("Autor");
    }
}