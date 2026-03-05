using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public MySqlConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    // Métodos para Autor
    public async Task<int> InsertarAutor(Autor autor)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            @"INSERT INTO Autor (Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro) 
              VALUES (@Nombres, @Apellidos, @Nacionalidad, @FechaNacimiento, @Estado, @FechaRegistro);
              SELECT LAST_INSERT_ID();", 
            connection);

        command.Parameters.AddWithValue("@Nombres", autor.Nombres);
        command.Parameters.AddWithValue("@Apellidos", autor.Apellidos);
        command.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@FechaNacimiento", autor.FechaNacimiento ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", autor.Estado);
        command.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<Autor>> ObtenerAutores()
    {
        var autores = new List<Autor>();

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            "SELECT AutorId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro, UltimaActualizacion FROM autor ORDER BY AutorId DESC", 
            connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            autores.Add(new Autor
            {
                AutorId = reader.GetInt32("AutorId"),
                Nombres = reader.GetString("Nombres"),
                Apellidos = reader.GetString("Apellidos"),
                Nacionalidad = reader.IsDBNull(reader.GetOrdinal("Nacionalidad")) ? null : reader.GetString("Nacionalidad"),
                FechaNacimiento = reader.IsDBNull(reader.GetOrdinal("FechaNacimiento")) ? null : reader.GetDateTime("FechaNacimiento"),
                Estado = reader.GetBoolean("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("UltimaActualizacion")) ? null : reader.GetDateTime("UltimaActualizacion")
            });
        }

        return autores;
    }

    // Métodos para Libro
    public async Task<int> InsertarLibro(Libro libro)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            @"INSERT INTO libro (AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro) 
              VALUES (@AutorId, @Titulo, @Editorial, @Edicion, @AñoPublicacion, @Descripcion, @Estado, @FechaRegistro);
              SELECT LAST_INSERT_ID();", 
            connection);

        command.Parameters.AddWithValue("@AutorId", libro.AutorId);
        command.Parameters.AddWithValue("@Titulo", libro.Titulo);
        command.Parameters.AddWithValue("@Editorial", libro.Editorial ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Edicion", libro.Edicion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AñoPublicacion", libro.AñoPublicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Descripcion", libro.Descripcion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", libro.Estado);
        command.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<Libro>> ObtenerLibros()
    {
        var libros = new List<Libro>();

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            "SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion FROM libro ORDER BY LibroId DESC", 
            connection);

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

    public async Task<Libro?> ObtenerLibroPorId(int libroId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            "SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion FROM libro WHERE LibroId = @LibroId", 
            connection);

        command.Parameters.AddWithValue("@LibroId", libroId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
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

        return null;
    }

    public async Task<bool> ActualizarLibro(Libro libro)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            @"UPDATE libro 
              SET AutorId = @AutorId, 
                  Titulo = @Titulo, 
                  Editorial = @Editorial, 
                  Edicion = @Edicion, 
                  AñoPublicacion = @AñoPublicacion, 
                  Descripcion = @Descripcion, 
                  Estado = @Estado, 
                  UltimaActualizacion = @UltimaActualizacion
              WHERE LibroId = @LibroId", 
            connection);

        command.Parameters.AddWithValue("@LibroId", libro.LibroId);
        command.Parameters.AddWithValue("@AutorId", libro.AutorId);
        command.Parameters.AddWithValue("@Titulo", libro.Titulo);
        command.Parameters.AddWithValue("@Editorial", libro.Editorial ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Edicion", libro.Edicion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AñoPublicacion", libro.AñoPublicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Descripcion", libro.Descripcion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", libro.Estado);
        command.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> EliminarLibro(int libroId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            "DELETE FROM libro WHERE LibroId = @LibroId", 
            connection);

        command.Parameters.AddWithValue("@LibroId", libroId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> CambiarEstadoLibro(int libroId, bool estado)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            "UPDATE libro SET Estado = @Estado, UltimaActualizacion = @UltimaActualizacion WHERE LibroId = @LibroId", 
            connection);

        command.Parameters.AddWithValue("@LibroId", libroId);
        command.Parameters.AddWithValue("@Estado", estado);
        command.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // Métodos para Ejemplar (Inventario)
    public async Task<int> InsertarEjemplar(Ejemplar ejemplar)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            @"INSERT INTO ejemplar (LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro) 
              VALUES (@LibroId, @CodigoInventario, @EstadoConservacion, @Disponible, @DadoDeBaja, @MotivoBaja, @Ubicacion, @Estado, @FechaRegistro);
              SELECT LAST_INSERT_ID();", 
            connection);

        command.Parameters.AddWithValue("@LibroId", ejemplar.LibroId);
        command.Parameters.AddWithValue("@CodigoInventario", ejemplar.CodigoInventario);
        command.Parameters.AddWithValue("@EstadoConservacion", ejemplar.EstadoConservacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Disponible", ejemplar.Disponible);
        command.Parameters.AddWithValue("@DadoDeBaja", ejemplar.DadoDeBaja);
        command.Parameters.AddWithValue("@MotivoBaja", ejemplar.MotivoBaja ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Ubicacion", ejemplar.Ubicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", ejemplar.Estado);
        command.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<Ejemplar>> ObtenerEjemplares()
    {
        var ejemplares = new List<Ejemplar>();

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            "SELECT EjemplarId, LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro, UltimaActualizacion FROM ejemplar ORDER BY EjemplarId DESC", 
            connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ejemplares.Add(new Ejemplar
            {
                EjemplarId = reader.GetInt32("EjemplarId"),
                LibroId = reader.GetInt32("LibroId"),
                CodigoInventario = reader.GetString("CodigoInventario"),
                EstadoConservacion = reader.IsDBNull(reader.GetOrdinal("EstadoConservacion")) ? null : reader.GetString("EstadoConservacion"),
                Disponible = reader.GetBoolean("Disponible"),
                DadoDeBaja = reader.GetBoolean("DadoDeBaja"),
                MotivoBaja = reader.IsDBNull(reader.GetOrdinal("MotivoBaja")) ? null : reader.GetString("MotivoBaja"),
                Ubicacion = reader.IsDBNull(reader.GetOrdinal("Ubicacion")) ? null : reader.GetString("Ubicacion"),
                Estado = reader.GetBoolean("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("UltimaActualizacion")) ? null : reader.GetDateTime("UltimaActualizacion")
            });
        }

        return ejemplares;
    }

    public async Task<Ejemplar?> ObtenerEjemplarPorId(int ejemplarId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            "SELECT EjemplarId, LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro, UltimaActualizacion FROM ejemplar WHERE EjemplarId = @EjemplarId", 
            connection);

        command.Parameters.AddWithValue("@EjemplarId", ejemplarId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Ejemplar
            {
                EjemplarId = reader.GetInt32("EjemplarId"),
                LibroId = reader.GetInt32("LibroId"),
                CodigoInventario = reader.GetString("CodigoInventario"),
                EstadoConservacion = reader.IsDBNull(reader.GetOrdinal("EstadoConservacion")) ? null : reader.GetString("EstadoConservacion"),
                Disponible = reader.GetBoolean("Disponible"),
                DadoDeBaja = reader.GetBoolean("DadoDeBaja"),
                MotivoBaja = reader.IsDBNull(reader.GetOrdinal("MotivoBaja")) ? null : reader.GetString("MotivoBaja"),
                Ubicacion = reader.IsDBNull(reader.GetOrdinal("Ubicacion")) ? null : reader.GetString("Ubicacion"),
                Estado = reader.GetBoolean("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("UltimaActualizacion")) ? null : reader.GetDateTime("UltimaActualizacion")
            };
        }

        return null;
    }

    public async Task<bool> ActualizarEjemplar(Ejemplar ejemplar)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            @"UPDATE ejemplar 
              SET LibroId = @LibroId,
                  CodigoInventario = @CodigoInventario,
                  EstadoConservacion = @EstadoConservacion,
                  Disponible = @Disponible,
                  DadoDeBaja = @DadoDeBaja,
                  MotivoBaja = @MotivoBaja,
                  Ubicacion = @Ubicacion,
                  Estado = @Estado,
                  UltimaActualizacion = @UltimaActualizacion
              WHERE EjemplarId = @EjemplarId", 
            connection);

        command.Parameters.AddWithValue("@EjemplarId", ejemplar.EjemplarId);
        command.Parameters.AddWithValue("@LibroId", ejemplar.LibroId);
        command.Parameters.AddWithValue("@CodigoInventario", ejemplar.CodigoInventario);
        command.Parameters.AddWithValue("@EstadoConservacion", ejemplar.EstadoConservacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Disponible", ejemplar.Disponible);
        command.Parameters.AddWithValue("@DadoDeBaja", ejemplar.DadoDeBaja);
        command.Parameters.AddWithValue("@MotivoBaja", ejemplar.MotivoBaja ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Ubicacion", ejemplar.Ubicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", ejemplar.Estado);
        command.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> EliminarEjemplar(int ejemplarId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            "DELETE FROM ejemplar WHERE EjemplarId = @EjemplarId", 
            connection);

        command.Parameters.AddWithValue("@EjemplarId", ejemplarId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<string> ObtenerTituloLibro(int libroId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new MySqlCommand(
            "SELECT Titulo FROM libro WHERE LibroId = @LibroId", 
            connection);

        command.Parameters.AddWithValue("@LibroId", libroId);

        var result = await command.ExecuteScalarAsync();
        return result?.ToString() ?? "Libro no encontrado";
    }
}
