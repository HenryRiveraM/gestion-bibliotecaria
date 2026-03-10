using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;

namespace gestion_bibliotecaria.Pages;

public class InventarioCreateModel : PageModel
{
    private const string QueryLibros = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                                        FROM libro
                                        ORDER BY Titulo";

    private const string QueryInsertEjemplar = @"INSERT INTO ejemplar (LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro)
                                                 VALUES (@LibroId, @CodigoInventario, @EstadoConservacion, @Disponible, @DadoDeBaja, @MotivoBaja, @Ubicacion, @Estado, @FechaRegistro);
                                                 SELECT LAST_INSERT_ID();";

    private readonly IConfiguration _configuration;

    [BindProperty]
    public Ejemplar Ejemplar { get; set; } = new Ejemplar { Estado = true, Disponible = true };

    public List<Libro> Libros { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public InventarioCreateModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task OnGetAsync()
    {
        await CargarPaginaAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Ejemplar.CodigoInventario = (Ejemplar.CodigoInventario ?? string.Empty).Trim();
        Ejemplar.EstadoConservacion = (Ejemplar.EstadoConservacion ?? string.Empty).Trim();
        Ejemplar.Ubicacion = (Ejemplar.Ubicacion ?? string.Empty).Trim();
        Ejemplar.MotivoBaja = (Ejemplar.MotivoBaja ?? string.Empty).Trim();

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
            await CargarPaginaAsync();
            return Page();
        }

        try
        {
            await InsertarEjemplarAsync(Ejemplar);
            return Redirect("/Inventario");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al agregar el ejemplar: {ex.Message}";
            await CargarPaginaAsync();
            return Page();
        }
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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

    private async Task InsertarEjemplarAsync(Ejemplar ejemplar)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryInsertEjemplar, connection);
        command.Parameters.AddWithValue("@LibroId", ejemplar.LibroId);
        command.Parameters.AddWithValue("@CodigoInventario", ejemplar.CodigoInventario);
        command.Parameters.AddWithValue("@EstadoConservacion", ejemplar.EstadoConservacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Disponible", ejemplar.Disponible);
        command.Parameters.AddWithValue("@DadoDeBaja", ejemplar.DadoDeBaja);
        command.Parameters.AddWithValue("@MotivoBaja", ejemplar.MotivoBaja ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Ubicacion", ejemplar.Ubicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", ejemplar.Estado);
        command.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);

        await command.ExecuteScalarAsync();
    }

    private async Task CargarPaginaAsync()
    {
        Libros = await ObtenerLibrosAsync();
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
