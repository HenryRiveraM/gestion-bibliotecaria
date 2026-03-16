using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.Pages;

public class AutorEditModel : PageModel
{
    private const string QueryAutorPorId = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro, UltimaActualizacion
                                             FROM autor
                                             WHERE AutorId = @AutorId";

    private const string QueryUpdateAutor = @"UPDATE autor
                                              SET Nombres = @Nombres,
                                                  Apellidos = @Apellidos,
                                                  Nacionalidad = @Nacionalidad,
                                                  FechaNacimiento = @FechaNacimiento,
                                                  Estado = @Estado,
                                                  UltimaActualizacion = @UltimaActualizacion
                                              WHERE AutorId = @AutorId";

    private readonly IConfiguration _configuration;
    private readonly RouteTokenService _routeTokenService;
    private readonly IAutorFactory _autorFactory;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor();

    [BindProperty]
    public string AutorToken { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public AutorEditModel(
        IConfiguration configuration,
        RouteTokenService routeTokenService,
        IAutorFactory autorFactory)
    {
        _configuration = configuration;
        _routeTokenService = routeTokenService;
        _autorFactory = autorFactory;
    }

    public async Task<IActionResult> OnGetAsync(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        AutorToken = token;

        var autor = await ObtenerAutorPorIdAsync(id);

        if (autor == null)
        {
            return NotFound();
        }

        Autor = autor;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_routeTokenService.TryObtenerId(AutorToken, out var autorId))
        {
            return NotFound();
        }

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
            var autor = _autorFactory.CreateForUpdate(
                autorId,
                Autor.Nombres,
                Autor.Apellidos,
                Autor.Nacionalidad,
                Autor.FechaNacimiento,
                Autor.Estado
            );

            await ActualizarAutorAsync(autor);

            return RedirectToPage("Autor");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al actualizar el autor: {ex.Message}";
            return Page();
        }
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private async Task<Autor?> ObtenerAutorPorIdAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryAutorPorId, connection);
        command.Parameters.AddWithValue("@AutorId", id);

        using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapAutor(reader);
    }

    private async Task<bool> ActualizarAutorAsync(Autor autor)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryUpdateAutor, connection);

        command.Parameters.AddWithValue("@AutorId", autor.AutorId);
        command.Parameters.AddWithValue("@Nombres", autor.Nombres);
        command.Parameters.AddWithValue("@Apellidos", autor.Apellidos ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@FechaNacimiento", autor.FechaNacimiento ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", autor.Estado);
        command.Parameters.AddWithValue("@UltimaActualizacion", autor.UltimaActualizacion ?? DateTime.Now);

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    private static Autor MapAutor(MySqlDataReader reader)
    {
        return new Autor
        {
            AutorId = reader.GetInt32("AutorId"),
            Nombres = reader.GetString("Nombres"),
            Apellidos = reader.IsDBNull(reader.GetOrdinal("Apellidos")) ? null : reader.GetString("Apellidos"),
            Nacionalidad = reader.IsDBNull(reader.GetOrdinal("Nacionalidad")) ? null : reader.GetString("Nacionalidad"),
            FechaNacimiento = reader.IsDBNull(reader.GetOrdinal("FechaNacimiento")) ? null : reader.GetDateTime("FechaNacimiento"),
            Estado = reader.GetBoolean("Estado"),
            FechaRegistro = reader.GetDateTime("FechaRegistro"),
            UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("UltimaActualizacion")) ? null : reader.GetDateTime("UltimaActualizacion")
        };
    }
}