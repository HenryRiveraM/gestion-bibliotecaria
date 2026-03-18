using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class LibroCreateModel : PageModel
{
    private const string QueryAutores = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad
                                         FROM autor
                                         WHERE Estado = 1
                                         ORDER BY Apellidos, Nombres";

    private const string QueryInsertLibro = @"INSERT INTO libro (AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro)
                                              VALUES (@AutorId, @Titulo, @Editorial, @Edicion, @AñoPublicacion, @Descripcion, @Estado, @FechaRegistro)";

    private readonly IConfiguration _configuration;
    private readonly ILibroFactory _libroFactory;
    
    public LibroCreateModel(IConfiguration configuration, ILibroFactory libroFactory)
    {
        _configuration = configuration;
        _libroFactory = libroFactory;
    }
    
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
    public bool Estado { get; set; } = true;

    public DataTable Autores { get; set; } = new DataTable();

    public void OnGet()
    {
        CargarPagina();
    }

    public IActionResult OnPost()
    {
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
            CargarPagina();
            return Page();
        }

        var libro = _libroFactory.CreateForInsert(
            AutorId,
            Titulo,
            Editorial,
            Edicion,
            AñoPublicacion,
            Descripcion,
            Estado);

        InsertarLibro(libro);
        return Redirect("/Libro");
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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

    private void InsertarLibro(Libro libro)
    {
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryInsertLibro, connection))
            {
                command.Parameters.AddWithValue("@AutorId", libro.AutorId);
                command.Parameters.AddWithValue("@Titulo", libro.Titulo);
                command.Parameters.AddWithValue("@Editorial", libro.Editorial ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Edicion", libro.Edicion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AñoPublicacion", libro.AñoPublicacion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Descripcion", libro.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estado", libro.Estado);
                command.Parameters.AddWithValue("@FechaRegistro", libro.FechaRegistro);

                command.ExecuteNonQuery();
            }
        }
    }

    private void CargarPagina()
    {
        Autores = ObtenerAutores();
    }
}