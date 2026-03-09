using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;
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
        Autor.Nombres = (Autor.Nombres ?? string.Empty).Trim();
        Autor.Apellidos = (Autor.Apellidos ?? string.Empty).Trim();
        Autor.Nacionalidad = (Autor.Nacionalidad ?? string.Empty).Trim();

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

        if (ValidadorEntrada.EstaVacio(Autor.Apellidos))
        {
            ModelState.AddModelError("Autor.Apellidos", "Los apellidos son obligatorios.");
        }
        else
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

        // FechaNacimiento should not be in the future
        if (!ValidadorEntrada.FechaNoFutura(Autor.FechaNacimiento))
        {
            ModelState.AddModelError("Autor.FechaNacimiento", "La fecha de nacimiento no puede ser futura.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

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