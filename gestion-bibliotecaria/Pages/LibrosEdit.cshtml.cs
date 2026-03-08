using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class LibrosEditModel : PageModel
{
    private readonly IConfiguration _configuration;

    [BindProperty]
    public Libro Libro { get; set; } = new Libro();

    public List<Autor> Autores { get; set; } = new List<Autor>();

    public string ErrorMessage { get; set; } = string.Empty;

    public LibrosEditModel(IConfiguration configuration)
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
        Autores = await ObtenerAutoresAsync();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor completa todos los campos requeridos.";
            Autores = await ObtenerAutoresAsync();
            return Page();
        }

        try
        {
            var success = await ActualizarLibroAsync(Libro);
            
            if (success)
            {
                return RedirectToPage("Libros");
            }
            else
            {
                ErrorMessage = "No se pudo actualizar el libro.";
                Autores = await ObtenerAutoresAsync();
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al actualizar el libro: {ex.Message}";
            Autores = await ObtenerAutoresAsync();
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

    private async Task<List<Autor>> ObtenerAutoresAsync()
    {
        var autores = new List<Autor>();

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro, UltimaActualizacion
                               FROM autor
                               ORDER BY Apellidos, Nombres";
        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            autores.Add(new Autor
            {
                AutorId = reader.GetInt32("AutorId"),
                Nombres = reader.GetString("Nombres"),
                Apellidos = reader.GetString("Apellidos"),
                Nacionalidad = reader.IsDBNull(reader.GetOrdinal("Nacionalidad")) ? null : reader.GetString("Nacionalidad"),
                FechaNacimiento = reader.IsDBNull(reader.GetOrdinal("FechaNacimiento")) ? null : reader.GetDateTime("FechaNacimiento"),
                Estado = reader.GetBoolean("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("UltimaActualizacion")) ? null : reader.GetDateTime("UltimaActualizacion")
            });
        }

        return autores;
    }

    private async Task<bool> ActualizarLibroAsync(Libro libro)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = @"UPDATE libro
                               SET AutorId = @AutorId,
                                   Titulo = @Titulo,
                                   Editorial = @Editorial,
                                   Edicion = @Edicion,
                                   AñoPublicacion = @AñoPublicacion,
                                   Descripcion = @Descripcion,
                                   Estado = @Estado,
                                   UltimaActualizacion = @UltimaActualizacion
                               WHERE LibroId = @LibroId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@LibroId", libro.LibroId);
        command.Parameters.AddWithValue("@AutorId", libro.AutorId);
        command.Parameters.AddWithValue("@Titulo", libro.Titulo);
        command.Parameters.AddWithValue("@Editorial", libro.Editorial ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Edicion", libro.Edicion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AñoPublicacion", libro.AñoPublicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Descripcion", libro.Descripcion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", libro.Estado);
        command.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
