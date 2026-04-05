using System.Data;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;

namespace gestion_bibliotecaria.Infrastructure.Persistence;

public class LibroRepository : ILibroRepositorio, IRepository<Libro, int>
{
    private readonly string _connectionString;

    public LibroRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public LibroRepository(IConfiguration configuration)
        : this(configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
    {
    }

    private string ConnectionString => _connectionString;

    private const string QueryLibros = @"
        SELECT LibroId,
               AutorId,
               Titulo,
               ISBN,
               Editorial,
               Genero,
               Edicion,
               AñoPublicacion,
               NumeroPaginas,
               Idioma,
               PaisPublicacion,
               Descripcion,
               Estado
        FROM libro
        ORDER BY Titulo ASC";

    private const string QueryAutores = "SELECT AutorId, Nombres, Apellidos FROM autor";

    private const string QueryAutoresActivos = @"
        SELECT AutorId, Nombres, Apellidos, Nacionalidad
        FROM autor
        WHERE Estado = 1
        ORDER BY Apellidos, Nombres";

    private const string QueryLibroPorId = @"
        SELECT LibroId,
               AutorId,
               Titulo,
               ISBN,
               Editorial,
               Genero,
               Edicion,
               AñoPublicacion,
               NumeroPaginas,
               Idioma,
               PaisPublicacion,
               Descripcion,
               Estado
        FROM libro
        WHERE LibroId = @LibroId";

    private const string QueryNombreAutor = @"
        SELECT CONCAT(Nombres, ' ', Apellidos) AS NombreCompleto
        FROM autor
        WHERE AutorId = @AutorId";

    private const string QueryInsertLibro = @"
        INSERT INTO libro
        (
            AutorId,
            Titulo,
            ISBN,
            Editorial,
            Genero,
            Edicion,
            AñoPublicacion,
            NumeroPaginas,
            Idioma,
            PaisPublicacion,
            Descripcion,
            Estado,
            FechaRegistro
        )
        VALUES
        (
            @AutorId,
            @Titulo,
            @ISBN,
            @Editorial,
            @Genero,
            @Edicion,
            @AñoPublicacion,
            @NumeroPaginas,
            @Idioma,
            @PaisPublicacion,
            @Descripcion,
            @Estado,
            @FechaRegistro
        )";

    private const string QueryUpdateLibro = @"
        UPDATE libro
        SET AutorId = @AutorId,
            Titulo = @Titulo,
            ISBN = @ISBN,
            Editorial = @Editorial,
            Genero = @Genero,
            Edicion = @Edicion,
            AñoPublicacion = @AñoPublicacion,
            NumeroPaginas = @NumeroPaginas,
            Idioma = @Idioma,
            PaisPublicacion = @PaisPublicacion,
            Descripcion = @Descripcion,
            Estado = @Estado,
            UltimaActualizacion = @UltimaActualizacion
        WHERE LibroId = @LibroId";

    private const string QueryDeleteLibro = @"
        UPDATE libro
        SET Estado = 0,
            UltimaActualizacion = @UltimaActualizacion
        WHERE LibroId = @LibroId";

    private const string QueryExisteAutorActivo = @"
        SELECT COUNT(1)
        FROM autor
        WHERE AutorId = @AutorId AND Estado = 1";
    
    private const string QueryInsertarAutor = @"
        INSERT INTO autor (Nombres, Apellidos, Estado, FechaRegistro)
        VALUES (@Nombres, @Apellidos, 1, @FechaRegistro);
        SELECT LAST_INSERT_ID();";

    public DataTable Select()
    {
        var dataTable = new DataTable();

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryLibros, connection);
        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dataTable);

        return dataTable;
    }

    public DataTable GetAll() => Select();

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
            ISBN = reader["ISBN"] == DBNull.Value ? null : reader["ISBN"].ToString(),
            Editorial = reader["Editorial"] == DBNull.Value ? null : reader["Editorial"].ToString(),
            Genero = reader["Genero"] == DBNull.Value ? null : reader["Genero"].ToString(),
            Edicion = reader["Edicion"] == DBNull.Value ? null : reader["Edicion"].ToString(),
            AñoPublicacion = reader["AñoPublicacion"] == DBNull.Value ? null : Convert.ToInt32(reader["AñoPublicacion"]),
            NumeroPaginas = reader["NumeroPaginas"] == DBNull.Value ? null : Convert.ToInt32(reader["NumeroPaginas"]),
            Idioma = reader["Idioma"] == DBNull.Value ? null : reader["Idioma"].ToString(),
            PaisPublicacion = reader["PaisPublicacion"] == DBNull.Value ? null : reader["PaisPublicacion"].ToString(),
            Descripcion = reader["Descripcion"] == DBNull.Value ? null : reader["Descripcion"].ToString(),
            Estado = Convert.ToBoolean(reader["Estado"]),
            FechaRegistro = DateTime.Now,
            UltimaActualizacion = null
        };
    }

    public void Create(Libro libro)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryInsertLibro, connection);

        command.Parameters.AddWithValue("@AutorId", libro.AutorId);
        command.Parameters.AddWithValue("@Titulo", libro.Titulo);
        command.Parameters.AddWithValue("@ISBN", string.IsNullOrWhiteSpace(libro.ISBN) ? DBNull.Value : libro.ISBN);
        command.Parameters.AddWithValue("@Editorial", string.IsNullOrWhiteSpace(libro.Editorial) ? DBNull.Value : libro.Editorial);
        command.Parameters.AddWithValue("@Genero", string.IsNullOrWhiteSpace(libro.Genero) ? DBNull.Value : libro.Genero);
        command.Parameters.AddWithValue("@Edicion", string.IsNullOrWhiteSpace(libro.Edicion) ? DBNull.Value : libro.Edicion);
        command.Parameters.AddWithValue("@AñoPublicacion", libro.AñoPublicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@NumeroPaginas", libro.NumeroPaginas ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Idioma", string.IsNullOrWhiteSpace(libro.Idioma) ? DBNull.Value : libro.Idioma);
        command.Parameters.AddWithValue("@PaisPublicacion", string.IsNullOrWhiteSpace(libro.PaisPublicacion) ? DBNull.Value : libro.PaisPublicacion);
        command.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(libro.Descripcion) ? DBNull.Value : libro.Descripcion);
        command.Parameters.AddWithValue("@Estado", libro.Estado);
        command.Parameters.AddWithValue("@FechaRegistro", libro.FechaRegistro);

        command.ExecuteNonQuery();
    }

    public void Insert(Libro libro) => Create(libro);

    public void Update(Libro libro)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryUpdateLibro, connection);

        command.Parameters.AddWithValue("@LibroId", libro.LibroId);
        command.Parameters.AddWithValue("@AutorId", libro.AutorId);
        command.Parameters.AddWithValue("@Titulo", libro.Titulo);
        command.Parameters.AddWithValue("@ISBN", string.IsNullOrWhiteSpace(libro.ISBN) ? DBNull.Value : libro.ISBN);
        command.Parameters.AddWithValue("@Editorial", string.IsNullOrWhiteSpace(libro.Editorial) ? DBNull.Value : libro.Editorial);
        command.Parameters.AddWithValue("@Genero", string.IsNullOrWhiteSpace(libro.Genero) ? DBNull.Value : libro.Genero);
        command.Parameters.AddWithValue("@Edicion", string.IsNullOrWhiteSpace(libro.Edicion) ? DBNull.Value : libro.Edicion);
        command.Parameters.AddWithValue("@AñoPublicacion", libro.AñoPublicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@NumeroPaginas", libro.NumeroPaginas ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Idioma", string.IsNullOrWhiteSpace(libro.Idioma) ? DBNull.Value : libro.Idioma);
        command.Parameters.AddWithValue("@PaisPublicacion", string.IsNullOrWhiteSpace(libro.PaisPublicacion) ? DBNull.Value : libro.PaisPublicacion);
        command.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(libro.Descripcion) ? DBNull.Value : libro.Descripcion);
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

    public bool ExisteAutorActivo(int autorId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryExisteAutorActivo, connection);
        command.Parameters.AddWithValue("@AutorId", autorId);

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result) > 0;
    }
    
    public int InsertarAutorYObtenerID(string nombreCompleto)
    {
        var partes = nombreCompleto.Trim().Split(' ', 2);
        var nombres = partes[0];
        var apellidos = partes.Length > 1 ? partes[1] : "";

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        using var command = new MySqlCommand(QueryInsertarAutor, connection);
        command.Parameters.AddWithValue("@Nombres", nombres);
        command.Parameters.AddWithValue("@Apellidos", apellidos);
        command.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);

        return Convert.ToInt32(command.ExecuteScalar());
    }
}