using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Pages;

public class EjemplarDeleteModel : PageModel
{
    private const string QueryEjemplarPorId = @"SELECT EjemplarId, LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro, UltimaActualizacion
                                                FROM ejemplar
                                                WHERE EjemplarId = @EjemplarId";

    private const string QueryTituloLibro = "SELECT Titulo FROM libro WHERE LibroId = @LibroId";
    private const string QueryDeleteEjemplar = "DELETE FROM ejemplar WHERE EjemplarId = @EjemplarId";

    private readonly IConfiguration _configuration;

    public Ejemplar Ejemplar { get; set; } = new Ejemplar();
    public string TituloLibro { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarDeleteModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await CargarDetalleEjemplarAsync(id))
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            var success = await EliminarEjemplarAsync(id);

            if (success)
            {
                return Redirect("/Ejemplar");
            }

            ErrorMessage = "No se pudo eliminar el ejemplar del modulo de ejemplares.";
            await CargarDetalleEjemplarAsync(id);
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al eliminar el ejemplar: {ex.Message}";
            await CargarDetalleEjemplarAsync(id);
            return Page();
        }
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private async Task<Ejemplar?> ObtenerEjemplarPorIdAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryEjemplarPorId, connection);
        command.Parameters.AddWithValue("@EjemplarId", id);

        using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapEjemplar(reader);
    }

    private async Task<string> ObtenerTituloLibroAsync(int libroId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryTituloLibro, connection);
        command.Parameters.AddWithValue("@LibroId", libroId);

        var result = await command.ExecuteScalarAsync();
        return result?.ToString() ?? "Libro no encontrado";
    }

    private async Task<bool> EliminarEjemplarAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryDeleteEjemplar, connection);
        command.Parameters.AddWithValue("@EjemplarId", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private async Task<bool> CargarDetalleEjemplarAsync(int id)
    {
        var ejemplar = await ObtenerEjemplarPorIdAsync(id);
        if (ejemplar == null)
        {
            return false;
        }

        Ejemplar = ejemplar;
        TituloLibro = await ObtenerTituloLibroAsync(ejemplar.LibroId);
        return true;
    }

    private static Ejemplar MapEjemplar(MySqlDataReader reader)
    {
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
}
