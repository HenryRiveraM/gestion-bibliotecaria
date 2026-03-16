using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.FactoryProducts;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Pages;

public class AutorCreateModel : PageModel
{
    private const string QueryInsertAutor = @"INSERT INTO autor (Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro)
                                              VALUES (@Nombres, @Apellidos, @Nacionalidad, @FechaNacimiento, @Estado, @FechaRegistro);
                                              SELECT LAST_INSERT_ID();";

    private readonly IConfiguration _configuration;
    private readonly IAutorFactory _autorFactory;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor { Estado = true };

    public string ErrorMessage { get; set; } = string.Empty;

    public AutorCreateModel(IConfiguration configuration, IAutorFactory autorFactory)
    {
        _configuration = configuration;
        _autorFactory = autorFactory;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPostAsync()
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
            ErrorMessage = "Por favor completa todos los campos requeridos.";
            return Page();
        }

        try
        {
            var autor = _autorFactory.CreateForInsert(
                Autor.Nombres,
                Autor.Apellidos,
                Autor.Nacionalidad,
                Autor.FechaNacimiento,
                Autor.Estado
            );

            await InsertarAutorAsync(autor);

            return RedirectToPage("Autor");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al agregar el autor: {ex.Message}";
            return Page();
        }
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private async Task InsertarAutorAsync(Autor autor)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryInsertAutor, connection);

        command.Parameters.AddWithValue("@Nombres", autor.Nombres);
        command.Parameters.AddWithValue("@Apellidos", autor.Apellidos ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@FechaNacimiento", autor.FechaNacimiento ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", autor.Estado);
        command.Parameters.AddWithValue("@FechaRegistro", autor.FechaRegistro);

        await command.ExecuteScalarAsync();
    }
}