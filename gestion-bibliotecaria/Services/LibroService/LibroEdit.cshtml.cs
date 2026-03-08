using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using MySql.Data.MySqlClient;

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

    [BindProperty]
    public int LibroId { get; set; }

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

    public DateTime FechaRegistro { get; set; }

    public DataTable Autores { get; set; } = new DataTable();

    public LibroEditModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult OnGet(int id)
    {
        if (!CargarPagina(id))
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        ActualizarLibro();
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

    private void ActualizarLibro()
    {
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryUpdateLibro, connection))
            {
                command.Parameters.AddWithValue("@LibroId", LibroId);
                command.Parameters.AddWithValue("@AutorId", AutorId);
                command.Parameters.AddWithValue("@Titulo", Titulo);
                command.Parameters.AddWithValue("@Editorial", Editorial ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Edicion", Edicion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AñoPublicacion", AñoPublicacion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Descripcion", Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estado", Estado);
                command.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);

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
