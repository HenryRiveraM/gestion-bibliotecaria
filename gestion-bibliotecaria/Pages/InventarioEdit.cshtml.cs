using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class InventarioEditModel : PageModel
{
    private readonly IConfiguration _configuration;

    [BindProperty]
    public Ejemplar Ejemplar { get; set; } = new Ejemplar();

    public List<Libro> Libros { get; set; } = new List<Libro>();

    public string ErrorMessage { get; set; } = string.Empty;

    public InventarioEditModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ejemplar = await ObtenerEjemplarPorIdAsync(id);
        
        if (ejemplar == null)
        {
            return NotFound();
        }

        Ejemplar = ejemplar;
        Libros = await ObtenerLibrosAsync();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor completa todos los campos requeridos.";
            Libros = await ObtenerLibrosAsync();
            return Page();
        }

        try
        {
            var success = await ActualizarEjemplarAsync(Ejemplar);
            
            if (success)
            {
                return RedirectToPage("Inventario");
            }
            else
            {
                ErrorMessage = "No se pudo actualizar el ejemplar.";
                Libros = await ObtenerLibrosAsync();
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al actualizar el ejemplar: {ex.Message}";
            Libros = await ObtenerLibrosAsync();
            return Page();
        }
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private async Task<Ejemplar?> ObtenerEjemplarPorIdAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = @"SELECT EjemplarId, LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro, UltimaActualizacion
                               FROM ejemplar
                               WHERE EjemplarId = @EjemplarId";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@EjemplarId", id);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Ejemplar
        {
            EjemplarId = reader.GetInt32("EjemplarId"),
            LibroId = reader.GetInt32("LibroId"),
            CodigoInventario = reader.GetString("CodigoInventario"),
            EstadoConservacion = reader.IsDBNull(reader.GetOrdinal("EstadoConservacion")) ? null : reader.GetString("EstadoConservacion"),
            Disponible = reader.GetBoolean("Disponible"),
            DadoDeBaja = reader.GetBoolean("DadoDeBaja"),
            MotivoBaja = reader.IsDBNull(reader.GetOrdinal("MotivoBaja")) ? null : reader.GetString("MotivoBaja"),
            Ubicacion = reader.IsDBNull(reader.GetOrdinal("Ubicacion")) ? null : reader.GetString("Ubicacion"),
            Estado = reader.GetBoolean("Estado"),
            FechaRegistro = reader.GetDateTime("FechaRegistro"),
            UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("UltimaActualizacion")) ? null : reader.GetDateTime("UltimaActualizacion")
        };
    }

    private async Task<List<Libro>> ObtenerLibrosAsync()
    {
        var libros = new List<Libro>();

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                               FROM libro
                               ORDER BY Titulo";
        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            libros.Add(new Libro
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
            });
        }

        return libros;
    }

    private async Task<bool> ActualizarEjemplarAsync(Ejemplar ejemplar)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = @"UPDATE ejemplar
                               SET LibroId = @LibroId,
                                   CodigoInventario = @CodigoInventario,
                                   EstadoConservacion = @EstadoConservacion,
                                   Disponible = @Disponible,
                                   DadoDeBaja = @DadoDeBaja,
                                   MotivoBaja = @MotivoBaja,
                                   Ubicacion = @Ubicacion,
                                   Estado = @Estado,
                                   UltimaActualizacion = @UltimaActualizacion
                               WHERE EjemplarId = @EjemplarId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@EjemplarId", ejemplar.EjemplarId);
        command.Parameters.AddWithValue("@LibroId", ejemplar.LibroId);
        command.Parameters.AddWithValue("@CodigoInventario", ejemplar.CodigoInventario);
        command.Parameters.AddWithValue("@EstadoConservacion", ejemplar.EstadoConservacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Disponible", ejemplar.Disponible);
        command.Parameters.AddWithValue("@DadoDeBaja", ejemplar.DadoDeBaja);
        command.Parameters.AddWithValue("@MotivoBaja", ejemplar.MotivoBaja ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Ubicacion", ejemplar.Ubicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", ejemplar.Estado);
        command.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
