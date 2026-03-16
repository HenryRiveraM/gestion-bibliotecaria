using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;

namespace gestion_bibliotecaria.Pages;

public class LibroEditModel : PageModel
{
    private const string QueryLibroPorId = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                                             FROM libro
                                             WHERE LibroId = @LibroId";

    private const string QueryAutores = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad
                                         FROM autor
                                         WHERE Estado = 1
                                         ORDER BY Apellidos, Nombres";

    private const string QueryUpdateLibro = @"UPDATE libro
                                              SET AutorId = @AutorId,
                                                  Titulo = @Titulo,
                                                  Editorial = @Editorial,
                                                  Edicion = @Edicion,
                                                  AñoPublicacion = @AñoPublicacion,
                                                  Descripcion = @Descripcion,
                                                  Estado = @Estado,
                                                  UltimaActualizacion = @UltimaActualizacion
                                              WHERE LibroId = @LibroId";

    private readonly IConfiguration _configuration;
    private readonly ILibroFactory _libroFactory;
    private readonly RouteTokenService _routeTokenService;

    [BindProperty]
    public int LibroId { get; set; }

    [BindProperty]
    public string LibroToken { get; set; } = string.Empty;

    [BindProperty]
    public int AutorId { get; set; }

    [BindProperty]
    public string Titulo { get; set; } = string.Empty;

    [BindProperty]
    public string? Editorial { get; set; }

    [BindProperty]
    public string? Edicion { get; set; }

    [BindProperty]
    public int? AñoPublicacion { get; set; }

    [BindProperty]
    public string? Descripcion { get; set; }

    [BindProperty]
    public bool Estado { get; set; }

    [BindProperty]
    public DateTime FechaRegistro { get; set; }

    public DataTable Autores { get; set; } = new DataTable();

    public LibroEditModel(
        IConfiguration configuration,
        ILibroFactory libroFactory,
        RouteTokenService routeTokenService)
    {
        _configuration = configuration;
        _libroFactory = libroFactory;
        _routeTokenService = routeTokenService;
    }

    public IActionResult OnGet(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        if (!CargarPagina(id))
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!_routeTokenService.TryObtenerId(LibroToken, out var id))
        {
            return NotFound();
        }

        LibroId = id;

        Titulo = ValidadorEntrada.NormalizarEspacios(Titulo);
        Editorial = ValidadorEntrada.NormalizarEspacios(Editorial);
        Edicion = ValidadorEntrada.NormalizarEspacios(Edicion);
        Descripcion = ValidadorEntrada.NormalizarEspacios(Descripcion);

        if (ValidadorEntrada.EstaVacio(Titulo))
        {
            ModelState.AddModelError("Titulo", "El título es obligatorio.");
        }
        else if (ValidadorEntrada.ExcedeLongitud(Titulo, 100))
        {
            ModelState.AddModelError("Titulo", "El título excede la longitud máxima de 100 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(Editorial))
        {
            if (ValidadorEntrada.ExcedeLongitud(Editorial, 100))
            {
                ModelState.AddModelError("Editorial", "La editorial excede la longitud máxima de 100 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Edicion))
        {
            if (ValidadorEntrada.ExcedeLongitud(Edicion, 50))
            {
                ModelState.AddModelError("Edicion", "La edición excede la longitud máxima de 50 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Descripcion))
        {
            if (ValidadorEntrada.ExcedeLongitud(Descripcion, 500))
            {
                ModelState.AddModelError("Descripcion", "La descripción excede la longitud máxima de 500 caracteres.");
            }
        }

        if (!ValidadorEntrada.ValidYear(AñoPublicacion))
        {
            ModelState.AddModelError("AñoPublicacion", "El año de publicación no es válido.");
        }

        if (!ModelState.IsValid)
        {
            Autores = ObtenerAutores();
            return Page();
        }

        var libro = _libroFactory.CreateForUpdate(
            LibroId,
            AutorId,
            Titulo,
            Editorial,
            Edicion,
            AñoPublicacion,
            Descripcion,
            Estado);

        ActualizarLibro(libro);
        return Redirect("/Libro");
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private DataRow? ObtenerLibroPorId(int id)
    {
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryLibroPorId, connection))
            {
                command.Parameters.AddWithValue("@LibroId", id);

                using (var adapter = new MySqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataTable.Rows.Count > 0)
                    {
                        return dataTable.Rows[0];
                    }
                }
            }
        }

        return null;
    }

    private DataTable ObtenerAutores()
    {
        var dataTable = new DataTable();

        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryAutores, connection))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
        }

        return dataTable;
    }

    private void ActualizarLibro(Libro libro)
    {
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryUpdateLibro, connection))
            {
                command.Parameters.AddWithValue("@LibroId", libro.LibroId);
                command.Parameters.AddWithValue("@AutorId", libro.AutorId);
                command.Parameters.AddWithValue("@Titulo", libro.Titulo);
                command.Parameters.AddWithValue("@Editorial", libro.Editorial ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Edicion", libro.Edicion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AñoPublicacion", libro.AñoPublicacion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Descripcion", libro.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estado", libro.Estado);
                command.Parameters.AddWithValue("@UltimaActualizacion", libro.UltimaActualizacion ?? DateTime.Now);

                command.ExecuteNonQuery();
            }
        }
    }

    private bool CargarPagina(int id)
    {
        var libro = ObtenerLibroPorId(id);
        if (libro == null)
        {
            return false;
        }

        LibroId = libro.Field<int>("LibroId");
        LibroToken = _routeTokenService.CrearToken(LibroId);
        AutorId = libro.Field<int>("AutorId");
        Titulo = libro.Field<string>("Titulo") ?? string.Empty;
        Editorial = libro.Field<string>("Editorial");
        Edicion = libro.Field<string>("Edicion");
        AñoPublicacion = libro.Field<int?>("AñoPublicacion");
        Descripcion = libro.Field<string>("Descripcion");
        Estado = libro.Field<bool>("Estado");
        FechaRegistro = libro.Field<DateTime>("FechaRegistro");

        Autores = ObtenerAutores();
        return true;
    }
}