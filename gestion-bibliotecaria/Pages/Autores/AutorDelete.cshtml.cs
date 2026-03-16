using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;

namespace gestion_bibliotecaria.Pages;

public class AutorDeleteModel : PageModel
{
    private const string QueryAutorPorId = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro, UltimaActualizacion
                                             FROM autor
                                             WHERE AutorId = @AutorId";

    private const string QueryDeleteAutor = "DELETE FROM autor WHERE AutorId = @AutorId";

    private readonly IConfiguration _configuration;
    private readonly RouteTokenService _routeTokenService;

    public Autor Autor { get; set; } = new Autor();

    [BindProperty]
    public string AutorToken { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public AutorDeleteModel(IConfiguration configuration, RouteTokenService routeTokenService)
    {
        _configuration = configuration;
        _routeTokenService = routeTokenService;
    }

    public async Task<IActionResult> OnGetAsync(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        AutorToken = token;

        if (!await CargarDetalleAutorAsync(id))
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_routeTokenService.TryObtenerId(AutorToken, out var id))
        {
            return NotFound();
        }

        try
        {
            var success = await EliminarAutorAsync(id);

            if (success)
            {
                return Redirect("/Autores/Autor");
            }

            ErrorMessage = "No se pudo eliminar el autor.";
            await CargarDetalleAutorAsync(id);
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al eliminar el autor: {ex.Message}";
            await CargarDetalleAutorAsync(id);
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

    private async Task<bool> EliminarAutorAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryDeleteAutor, connection);
        command.Parameters.AddWithValue("@AutorId", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private async Task<bool> CargarDetalleAutorAsync(int id)
    {
        var autor = await ObtenerAutorPorIdAsync(id);
        if (autor == null)
        {
            return false;
        }

        Autor = autor;
        return true;
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