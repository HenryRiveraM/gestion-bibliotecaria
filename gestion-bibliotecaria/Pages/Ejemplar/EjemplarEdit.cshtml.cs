using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;

namespace gestion_bibliotecaria.Pages;

public class EjemplarEditModel : PageModel
{
    private const string QueryEjemplarPorId = @"SELECT EjemplarId, LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro, UltimaActualizacion
                                                FROM ejemplar
                                                WHERE EjemplarId = @EjemplarId";

    private const string QueryLibros = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                                        FROM libro
                                        ORDER BY Titulo";

    private const string QueryUpdateEjemplar = @"UPDATE ejemplar
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

    private readonly IConfiguration _configuration;

    [BindProperty]
    public Ejemplar Ejemplar { get; set; } = new Ejemplar();

    public List<Libro> Libros { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarEditModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await CargarPaginaAsync(id))
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Normalize text inputs
        Ejemplar.CodigoInventario = ValidadorEntrada.NormalizarEspacios(Ejemplar.CodigoInventario);
        Ejemplar.EstadoConservacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.EstadoConservacion);
        Ejemplar.Ubicacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.Ubicacion);
        Ejemplar.MotivoBaja = ValidadorEntrada.NormalizarEspacios(Ejemplar.MotivoBaja);

        // Validate fields (same rules as create)
        if (ValidadorEntrada.EstaVacio(Ejemplar.CodigoInventario))
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "El código de inventario es obligatorio.");
        }
        else if (!ValidadorEntrada.CodigoInventarioValido(Ejemplar.CodigoInventario))
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "El código de inventario solo puede contener letras, números y guiones.");
        }
        else if (ValidadorEntrada.ExcedeLongitud(Ejemplar.CodigoInventario, 30))
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "El código de inventario excede la longitud máxima de 30 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(Ejemplar.EstadoConservacion))
        {
            if (ValidadorEntrada.ExcedeLongitud(Ejemplar.EstadoConservacion, 50))
            {
                ModelState.AddModelError("Ejemplar.EstadoConservacion", "El estado de conservación excede la longitud máxima de 50 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Ejemplar.Ubicacion))
        {
            if (ValidadorEntrada.ExcedeLongitud(Ejemplar.Ubicacion, 100))
            {
                ModelState.AddModelError("Ejemplar.Ubicacion", "La ubicación excede la longitud máxima de 100 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Ejemplar.MotivoBaja))
        {
            if (ValidadorEntrada.ExcedeLongitud(Ejemplar.MotivoBaja, 200))
            {
                ModelState.AddModelError("Ejemplar.MotivoBaja", "El motivo de baja excede la longitud máxima de 200 caracteres.");
            }
        }

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
                return Redirect("/Ejemplar");
            }

            ErrorMessage = "No se pudo actualizar el ejemplar.";
            Libros = await ObtenerLibrosAsync();
            return Page();
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

        using var command = new MySqlCommand(QueryEjemplarPorId, connection);
        command.Parameters.AddWithValue("@EjemplarId", id);

        using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapEjemplar(reader);
    }

    private async Task<List<Libro>> ObtenerLibrosAsync()
    {
        var libros = new List<Libro>();

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryLibros, connection);
        using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            libros.Add(MapLibro(reader));
        }

        return libros;
    }

    private async Task<bool> ActualizarEjemplarAsync(Ejemplar ejemplar)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryUpdateEjemplar, connection);
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

    private async Task<bool> CargarPaginaAsync(int id)
    {
        var ejemplar = await ObtenerEjemplarPorIdAsync(id);
        if (ejemplar == null)
        {
            return false;
        }

        Ejemplar = ejemplar;
        Libros = await ObtenerLibrosAsync();
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

    private static Libro MapLibro(MySqlDataReader reader)
    {
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
}
