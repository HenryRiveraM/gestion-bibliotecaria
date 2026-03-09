using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Pages;

public class LibroDeleteModel : PageModel
{
    private const string QueryLibroPorId = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                                             FROM libro
                                             WHERE LibroId = @LibroId";

    private const string QueryNombreAutor = "SELECT CONCAT(Nombres, ' ', Apellidos) AS NombreCompleto FROM autor WHERE AutorId = @AutorId";
    private const string QueryDeleteLibro = "UPDATE libro SET Estado = 0, UltimaActualizacion = @UltimaActualizacion WHERE LibroId = @LibroId";

    private readonly IConfiguration _configuration;

    public int LibroId { get; set; }
    public int AutorId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Editorial { get; set; }
    public string? Edicion { get; set; }
    public int? AñoPublicacion { get; set; }
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }

    public string NombreAutor { get; set; } = string.Empty;

    public LibroDeleteModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult OnGet(int id)
    {
        if (!CargarDetalleLibro(id))
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost(int id)
    {
        EliminarLibro(id);
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

    private string ObtenerNombreAutor(int autorId)
    {
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryNombreAutor, connection))
            {
                command.Parameters.AddWithValue("@AutorId", autorId);

                var result = command.ExecuteScalar();
                return result?.ToString() ?? "Autor no encontrado";
            }
        }
    }

    private void EliminarLibro(int id)
    {
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(QueryDeleteLibro, connection))
            {
                command.Parameters.AddWithValue("@LibroId", id);
                command.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);
                command.ExecuteNonQuery();
            }
        }
    }

    private bool CargarDetalleLibro(int id)
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
        UltimaActualizacion = libro.Field<DateTime?>("UltimaActualizacion");

        NombreAutor = ObtenerNombreAutor(AutorId);
        return true;
    }
}
