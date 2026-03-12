using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;
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
        Autor.Nombres = ValidadorEntrada.NormalizarEspacios(Autor.Nombres);
        Autor.Apellidos = ValidadorEntrada.NormalizarEspacios(Autor.Apellidos);
        Autor.Nacionalidad = ValidadorEntrada.NormalizarEspacios(Autor.Nacionalidad);

        if (ValidadorEntrada.EstaVacio(Autor.Nombres))
        {
            ModelState.AddModelError("Autor.Nombres", "Los nombres son obligatorios.");
        }
        else
        {
            if (!ValidadorEntrada.SoloLetras(Autor.Nombres))
            {
                ModelState.AddModelError("Autor.Nombres", "Los nombres solo pueden contener letras y espacios.");
            }
            else if (ValidadorEntrada.ExcedeLongitud(Autor.Nombres, 100))
            {
                ModelState.AddModelError("Autor.Nombres", "Los nombres exceden la longitud máxima de 100 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Autor.Apellidos))
        {
            if (!ValidadorEntrada.SoloLetras(Autor.Apellidos))
            {
                ModelState.AddModelError("Autor.Apellidos", "Los apellidos solo pueden contener letras y espacios.");
            }
            else if (ValidadorEntrada.ExcedeLongitud(Autor.Apellidos, 100))
            {
                ModelState.AddModelError("Autor.Apellidos", "Los apellidos exceden la longitud máxima de 100 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Autor.Nacionalidad))
        {
            if (!ValidadorEntrada.SoloLetras(Autor.Nacionalidad))
            {
                ModelState.AddModelError("Autor.Nacionalidad", "La nacionalidad solo puede contener letras y espacios.");
            }
            else if (ValidadorEntrada.ExcedeLongitud(Autor.Nacionalidad, 100))
            {
                ModelState.AddModelError("Autor.Nacionalidad", "La nacionalidad excede la longitud máxima de 100 caracteres.");
            }
        }

        if (!ValidadorEntrada.FechaNoFutura(Autor.FechaNacimiento))
        {
            ModelState.AddModelError("Autor.FechaNacimiento", "La fecha de nacimiento no puede ser futura.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

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