using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.Pages;

public class EjemplarCreateModel : PageModel
{
    private const string QueryLibros = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                                        FROM libro
                                        ORDER BY Titulo";

    private const string QueryInsertEjemplar = @"INSERT INTO ejemplar (LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro)
                                                 VALUES (@LibroId, @CodigoInventario, @EstadoConservacion, @Disponible, @DadoDeBaja, @MotivoBaja, @Ubicacion, @Estado, @FechaRegistro);
                                                 SELECT LAST_INSERT_ID();";

    private readonly IConfiguration _configuration;
    private readonly IEjemplarFactory _ejemplarFactory;

    [BindProperty]
    public Ejemplar Ejemplar { get; set; } = new Ejemplar { Estado = true, Disponible = true };

    public List<Libro> Libros { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarCreateModel(IConfiguration configuration, IEjemplarFactory ejemplarFactory)
    {
        _configuration = configuration;
        _ejemplarFactory = ejemplarFactory;
    }

    public async Task OnGetAsync()
    {
        await CargarPaginaAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Ejemplar.CodigoInventario = ValidadorEntrada.NormalizarEspacios(Ejemplar.CodigoInventario);
        Ejemplar.EstadoConservacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.EstadoConservacion);
        Ejemplar.Ubicacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.Ubicacion);
        Ejemplar.MotivoBaja = ValidadorEntrada.NormalizarEspacios(Ejemplar.MotivoBaja);

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

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor completa todos los campos requeridos.";
            await CargarPaginaAsync();
            return Page();
        }

        try
        {
            var ejemplar = _ejemplarFactory.CreateForInsert(
                Ejemplar.LibroId,
                Ejemplar.CodigoInventario,
                Ejemplar.EstadoConservacion,
                Ejemplar.Disponible,
                Ejemplar.DadoDeBaja,
                Ejemplar.MotivoBaja,
                Ejemplar.Ubicacion,
                Ejemplar.Estado
            );

            await InsertarEjemplarAsync(ejemplar);

            return Redirect("/Ejemplar");
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
        command.Parameters.AddWithValue("@FechaRegistro", ejemplar.FechaRegistro);

        await command.ExecuteScalarAsync();
    }

    private async Task CargarPaginaAsync()
    {
        Libros = await ObtenerLibrosAsync();
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