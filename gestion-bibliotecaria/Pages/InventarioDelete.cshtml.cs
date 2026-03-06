using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class InventarioDeleteModel : PageModel
{
    private readonly IConfiguration _configuration;

    public Ejemplar Ejemplar { get; set; } = new Ejemplar();
    public string TituloLibro { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public InventarioDeleteModel(IConfiguration configuration)
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
        TituloLibro = await ObtenerTituloLibroAsync(ejemplar.LibroId);
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            var success = await EliminarEjemplarAsync(id);
            
            if (success)
            {
                return RedirectToPage("Inventario");
            }
            else
            {
                ErrorMessage = "No se pudo eliminar el ejemplar del inventario.";
                var ejemplar = await ObtenerEjemplarPorIdAsync(id);
                if (ejemplar != null)
                {
                    Ejemplar = ejemplar;
                    TituloLibro = await ObtenerTituloLibroAsync(ejemplar.LibroId);
                }
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al eliminar el ejemplar: {ex.Message}";
            var ejemplar = await ObtenerEjemplarPorIdAsync(id);
            if (ejemplar != null)
            {
                Ejemplar = ejemplar;
                TituloLibro = await ObtenerTituloLibroAsync(ejemplar.LibroId);
            }
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

    private async Task<string> ObtenerTituloLibroAsync(int libroId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = "SELECT Titulo FROM libro WHERE LibroId = @LibroId";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@LibroId", libroId);

        var result = await command.ExecuteScalarAsync();
        return result?.ToString() ?? "Libro no encontrado";
    }

    private async Task<bool> EliminarEjemplarAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = "DELETE FROM ejemplar WHERE EjemplarId = @EjemplarId";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@EjemplarId", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
