using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class LibrosDeleteModel : PageModel
{
    private readonly IConfiguration _configuration;

    public Libro Libro { get; set; } = new Libro();
    public string NombreAutor { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public LibrosDeleteModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var libro = await ObtenerLibroPorIdAsync(id);
        
        if (libro == null)
        {
            return NotFound();
        }

        Libro = libro;
        NombreAutor = await ObtenerNombreAutorAsync(libro.AutorId);
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            var success = await EliminarLibroAsync(id);
            
            if (success)
            {
                return RedirectToPage("Libros");
            }
            else
            {
                ErrorMessage = "No se pudo eliminar el libro.";
                var libro = await ObtenerLibroPorIdAsync(id);
                if (libro != null)
                {
                    Libro = libro;
                    NombreAutor = await ObtenerNombreAutorAsync(libro.AutorId);
                }
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al eliminar el libro: {ex.Message}";
            var libro = await ObtenerLibroPorIdAsync(id);
            if (libro != null)
            {
                Libro = libro;
                NombreAutor = await ObtenerNombreAutorAsync(libro.AutorId);
            }
            return Page();
        }
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private async Task<Libro?> ObtenerLibroPorIdAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                               FROM libro
                               WHERE LibroId = @LibroId";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@LibroId", id);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Libro
        {
            LibroId = reader.GetInt32("LibroId"),
            AutorId = reader.GetInt32("AutorId"),
            Titulo = reader.GetString("Titulo"),
            Editorial = reader.IsDBNull(reader.GetOrdinal("Editorial")) ? null : reader.GetString("Editorial"),
            Edicion = reader.IsDBNull(reader.GetOrdinal("Edicion")) ? null : reader.GetString("Edicion"),
            AñoPublicacion = reader.IsDBNull(reader.GetOrdinal("AñoPublicacion")) ? null : reader.GetInt32("AñoPublicacion"),
            Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
            Estado = reader.GetBoolean("Estado"),
            FechaRegistro = reader.GetDateTime("FechaRegistro"),
            UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("UltimaActualizacion")) ? null : reader.GetDateTime("UltimaActualizacion")
        };
    }

    private async Task<string> ObtenerNombreAutorAsync(int autorId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = "SELECT Nombres, Apellidos FROM autor WHERE AutorId = @AutorId";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@AutorId", autorId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return $"{reader.GetString("Nombres")} {reader.GetString("Apellidos")}";
        }

        return "Autor no encontrado";
    }

    private async Task<bool> EliminarLibroAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = "DELETE FROM libro WHERE LibroId = @LibroId";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@LibroId", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
