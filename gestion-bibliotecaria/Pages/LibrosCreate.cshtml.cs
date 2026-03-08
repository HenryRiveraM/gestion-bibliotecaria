using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class LibrosCreateModel : PageModel
{
    private readonly IConfiguration _configuration;

    [BindProperty]
    public Libro Libro { get; set; } = new Libro { Estado = true };

    public List<Autor> Autores { get; set; } = new List<Autor>();

    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public LibrosCreateModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task OnGetAsync()
    {
        Autores = await ObtenerAutoresAsync();
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
            var libroId = await InsertarLibroAsync(Libro);
            SuccessMessage = $"Libro agregado exitosamente con ID: {libroId}";
            
            return RedirectToPage("Libros");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al agregar el libro: {ex.Message}";
            Autores = await ObtenerAutoresAsync();
            return Page();
        }
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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

    private async Task<int> InsertarLibroAsync(Libro libro)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = @"INSERT INTO libro (AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro)
                               VALUES (@AutorId, @Titulo, @Editorial, @Edicion, @AñoPublicacion, @Descripcion, @Estado, @FechaRegistro);
                               SELECT LAST_INSERT_ID();";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@AutorId", libro.AutorId);
        command.Parameters.AddWithValue("@Titulo", libro.Titulo);
        command.Parameters.AddWithValue("@Editorial", libro.Editorial ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Edicion", libro.Edicion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AñoPublicacion", libro.AñoPublicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Descripcion", libro.Descripcion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", libro.Estado);
        command.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
