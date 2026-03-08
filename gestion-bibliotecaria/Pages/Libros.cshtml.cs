using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class LibrosModel : PageModel
{
    private readonly IConfiguration _configuration;

    public List<Libro> Libros { get; set; } = new List<Libro>();
    public Dictionary<int, string> AutoresNombres { get; set; } = new Dictionary<int, string>();

    public LibrosModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task OnGetAsync()
    {
        Libros = await ObtenerLibrosAsync();
        AutoresNombres = await ObtenerNombresAutoresAsync();
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private async Task<List<Libro>> ObtenerLibrosAsync()
    {
        var libros = new List<Libro>();

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                               FROM libro
                               ORDER BY LibroId DESC";

        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            libros.Add(new Libro
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
            });
        }

        return libros;
    }

    private async Task<Dictionary<int, string>> ObtenerNombresAutoresAsync()
    {
        var nombres = new Dictionary<int, string>();

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = "SELECT AutorId, Nombres, Apellidos FROM autor";
        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var nombreCompleto = $"{reader.GetString("Nombres")} {reader.GetString("Apellidos")}";
            nombres[reader.GetInt32("AutorId")] = nombreCompleto;
        }

        return nombres;
    }
}
