using System.Data;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryProducts;

public class LibroRepository : IRepository<Libro, int>
{
    private readonly IConfiguration _configuration;

    public LibroRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private const string QueryLibros = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                                         FROM libro
                                         ORDER BY Titulo ASC";

    private const string QueryAutores = "SELECT AutorId, Nombres, Apellidos FROM autor";

    private const string QueryAutoresActivos = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad
                                                 FROM autor
                                                 WHERE Estado = 1
                                                 ORDER BY Apellidos, Nombres";

    private const string QueryLibroPorId = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                                             FROM libro
                                             WHERE LibroId = @LibroId";

    private const string QueryNombreAutor = "SELECT CONCAT(Nombres, ' ', Apellidos) AS NombreCompleto FROM autor WHERE AutorId = @AutorId";

    private const string QueryInsertLibro = @"INSERT INTO libro (AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro)
                                              VALUES (@AutorId, @Titulo, @Editorial, @Edicion, @AñoPublicacion, @Descripcion, @Estado, @FechaRegistro)";

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

    private const string QueryDeleteLibro = @"UPDATE libro
                                              SET Estado = 0,
                                                  UltimaActualizacion = @UltimaActualizacion
                                              WHERE LibroId = @LibroId";

    public DataTable GetAll()
    {
        var dataTable = new DataTable();

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryLibros, connection);
        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dataTable);

        return dataTable;
    }

    public Libro? GetById(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryLibroPorId, connection);
        command.Parameters.AddWithValue("@LibroId", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new Libro
        {
            LibroId = reader.GetInt32("LibroId"),
            AutorId = reader.GetInt32("AutorId"),
            Titulo = reader.GetString("Titulo"),
            Editorial = reader["Editorial"] == DBNull.Value ? null : reader["Editorial"].ToString(),
            Edicion = reader["Edicion"] == DBNull.Value ? null : reader["Edicion"].ToString(),
            AñoPublicacion = reader["AñoPublicacion"] == DBNull.Value ? null : Convert.ToInt32(reader["AñoPublicacion"]),
            Descripcion = reader["Descripcion"] == DBNull.Value ? null : reader["Descripcion"].ToString(),
            Estado = Convert.ToBoolean(reader["Estado"]),
            FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"]),
            UltimaActualizacion = reader["UltimaActualizacion"] == DBNull.Value
                ? null
                : Convert.ToDateTime(reader["UltimaActualizacion"])
        };
    }

    public void Insert(Libro libro)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryInsertLibro, connection);

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

    public void Update(Libro libro)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryUpdateLibro, connection);

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

    public void Delete(Libro libro)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryDeleteLibro, connection);

        command.Parameters.AddWithValue("@LibroId", libro.LibroId);
        command.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);

        command.ExecuteNonQuery();
    }

    public Dictionary<int, string> ObtenerNombresAutores()
    {
        var autores = new Dictionary<int, string>();

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryAutores, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var nombres = reader.GetString("Nombres");
            var apellidos = reader.GetString("Apellidos");
            autores[reader.GetInt32("AutorId")] = $"{nombres} {apellidos}";
        }

        return autores;
    }

    public DataTable ObtenerAutoresActivos()
    {
        var dataTable = new DataTable();

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryAutoresActivos, connection);
        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dataTable);

        return dataTable;
    }

    public string ObtenerNombreAutor(int autorId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryNombreAutor, connection);
        command.Parameters.AddWithValue("@AutorId", autorId);

        var result = command.ExecuteScalar();
        return result?.ToString() ?? "Autor no encontrado";
    }
}