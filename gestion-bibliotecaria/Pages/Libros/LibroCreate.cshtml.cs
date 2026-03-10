using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;

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

    public LibroCreateModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnGet()
    {
        CargarPagina();
    }

    public IActionResult OnPost()
    {
        Titulo = (Titulo ?? string.Empty).Trim();
        Editorial = (Editorial ?? string.Empty).Trim();
        Edicion = (Edicion ?? string.Empty).Trim();
        Descripcion = (Descripcion ?? string.Empty).Trim();

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

        if (!ModelState.IsValid)
        {
            CargarPagina();
            return Page();
        }

        InsertarLibro();
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

    private void InsertarLibro()
    {
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryInsertLibro, connection))
            {
                command.Parameters.AddWithValue("@AutorId", AutorId);
                command.Parameters.AddWithValue("@Titulo", Titulo);
                command.Parameters.AddWithValue("@Editorial", Editorial ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Edicion", Edicion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AñoPublicacion", AñoPublicacion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Descripcion", Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estado", Estado);
                command.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);

                command.ExecuteNonQuery();
            }
        }
    }

    private void CargarPagina()
    {
        Autores = ObtenerAutores();
    }
}
