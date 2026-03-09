using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class AutorEditModel : PageModel
{
    private readonly IConfiguration configuration;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor();

    public AutorEditModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void OnGet(int id)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        string query = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado
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
                    Autor.FechaNacimiento = reader["FechaNacimiento"] as DateTime?;
                    Autor.Estado = reader.GetBoolean("Estado");
                }
            }
        }
    }

    public IActionResult OnPost()
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        string query = @"UPDATE autor
                         SET
                            Nombres=@Nombres,
                            Apellidos=@Apellidos,
                            Nacionalidad=@Nacionalidad,
                            FechaNacimiento=@FechaNacimiento,
                            Estado=@Estado,
                            UltimaActualizacion=NOW()
                         WHERE AutorId=@AutorId";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@AutorId", Autor.AutorId);
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