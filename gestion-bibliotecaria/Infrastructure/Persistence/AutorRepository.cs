using System.Data;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Configuration;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Infrastructure.Persistence;

public class AutorRepository : IAutorRepositorio, IRepository<Autor, int>
{
    public AutorRepository()
    {
    }

    public IEnumerable<Autor> ObtenerAutoresActivos()
    {
        var autores = new List<Autor>();

        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        const string query = "SELECT AutorId, Nombres FROM autor WHERE Estado = 1";

        using var command = connection.CreateCommand();
        command.CommandText = query;
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var autor = new Autor
            {
                AutorId = reader.GetInt32(reader.GetOrdinal("AutorId")),
                Nombres = reader.GetString(reader.GetOrdinal("Nombres"))
            };
            autores.Add(autor);
        }

        return autores;
    }

    public IEnumerable<Autor> ObtenerAutoresActivosTabla()
    {
        var autores = new List<Autor>();

        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        const string query = @"SELECT AutorId, Nombres, Apellidos 
                               FROM autor 
                               WHERE Estado = 1 
                               ORDER BY Nombres ASC;";

        using var command = connection.CreateCommand();
        command.CommandText = query;
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var autor = new Autor
            {
                AutorId = reader.GetInt32(reader.GetOrdinal("AutorId")),
                Nombres = reader.GetString(reader.GetOrdinal("Nombres")),
                Apellidos = reader.IsDBNull(reader.GetOrdinal("Apellidos")) ? null : reader.GetString(reader.GetOrdinal("Apellidos"))
            };
            autores.Add(autor);
        }

        return autores;
    }

    public bool ExisteAutorActivo(int autorId)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        const string query = "SELECT COUNT(1) FROM autor WHERE AutorId = @AutorId AND Estado = 1";

        using var command = connection.CreateCommand();
        command.CommandText = query;
        
        var param = command.CreateParameter();
        param.ParameterName = "@AutorId";
        param.Value = autorId;
        command.Parameters.Add(param);

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result) > 0;
    }

    public void Create(Autor autor) => Insert(autor);

    public void DeleteById(int id)
    {
        Delete(new Autor { AutorId = id });
    }

    public IEnumerable<Autor> GetAll()
    {
        var autores = new List<Autor>();

        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"SELECT 
                             AutorId,
                             UsuarioSesionId,
                             Nombres,
                             Apellidos,
                             Nacionalidad,
                             FechaNacimiento,
                             Estado,
                             FechaRegistro,
                             UltimaActualizacion
                         FROM autor
                         ORDER BY Nombres ASC;";

        using var command = connection.CreateCommand();
        command.CommandText = query;
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            autores.Add(MapReaderToAutor(reader));
        }

        return autores;
    }

    public void Insert(Autor autor)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"INSERT INTO autor
            (UsuarioSesionId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro)
            VALUES
            (@UsuarioSesionId, UPPER(@Nombres), UPPER(@Apellidos), @Nacionalidad, @FechaNacimiento, @Estado, NOW());";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        AddParameter(command, "@UsuarioSesionId", autor.UsuarioSesionId ?? (object)DBNull.Value);
        AddParameter(command, "@Nombres", autor.Nombres);
        AddParameter(command, "@Apellidos", autor.Apellidos ?? (object)DBNull.Value);
        AddParameter(command, "@Nacionalidad", autor.Nacionalidad ?? (object)DBNull.Value);
        AddParameter(command, "@FechaNacimiento", autor.FechaNacimiento ?? (object)DBNull.Value);
        AddParameter(command, "@Estado", autor.Estado);

        command.ExecuteNonQuery();
    }

    public void Update(Autor autor)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"UPDATE autor
            SET UsuarioSesionId = @UsuarioSesionId,
                Nombres = UPPER(@Nombres),
                Apellidos = UPPER(@Apellidos),
                Nacionalidad = @Nacionalidad,
                FechaNacimiento = @FechaNacimiento,
                Estado = @Estado,
                UltimaActualizacion = NOW()
            WHERE AutorId = @AutorId;";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        AddParameter(command, "@AutorId", autor.AutorId);
        AddParameter(command, "@UsuarioSesionId", autor.UsuarioSesionId ?? (object)DBNull.Value);
        AddParameter(command, "@Nombres", autor.Nombres);
        AddParameter(command, "@Apellidos", autor.Apellidos ?? (object)DBNull.Value);
        AddParameter(command, "@Nacionalidad", autor.Nacionalidad ?? (object)DBNull.Value);
        AddParameter(command, "@FechaNacimiento", autor.FechaNacimiento ?? (object)DBNull.Value);
        AddParameter(command, "@Estado", autor.Estado);

        command.ExecuteNonQuery();
    }

    public void Delete(Autor autor)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"UPDATE autor 
                         SET Estado = 0, UsuarioSesionId = @UsuarioSesionId, UltimaActualizacion = NOW() 
                         WHERE AutorId = @AutorId;";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        AddParameter(command, "@AutorId", autor.AutorId);
        AddParameter(command, "@UsuarioSesionId", autor.UsuarioSesionId ?? (object)DBNull.Value);

        command.ExecuteNonQuery();
    }

    public Autor? GetById(int id)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = "SELECT * FROM autor WHERE AutorId = @AutorId;";

        using var command = connection.CreateCommand();
        command.CommandText = query;
        
        AddParameter(command, "@AutorId", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapReaderToAutor(reader);
        }

        return null;
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }

    private Autor MapReaderToAutor(IDataReader reader)
    {
        return new Autor
        {
            AutorId = reader.GetInt32(reader.GetOrdinal("AutorId")),
            UsuarioSesionId = reader.IsDBNull(reader.GetOrdinal("UsuarioSesionId")) ? null : reader.GetInt32(reader.GetOrdinal("UsuarioSesionId")),
            Nombres = reader.GetString(reader.GetOrdinal("Nombres")),
            Apellidos = reader.IsDBNull(reader.GetOrdinal("Apellidos")) ? null : reader.GetString(reader.GetOrdinal("Apellidos")),
            Nacionalidad = reader.IsDBNull(reader.GetOrdinal("Nacionalidad")) ? null : reader.GetString(reader.GetOrdinal("Nacionalidad")),
            FechaNacimiento = reader.IsDBNull(reader.GetOrdinal("FechaNacimiento")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaNacimiento")),
            Estado = reader.GetBoolean(reader.GetOrdinal("Estado")),
            FechaRegistro = reader.IsDBNull(reader.GetOrdinal("FechaRegistro")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("FechaRegistro")),
            UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("UltimaActualizacion")) ? null : reader.GetDateTime(reader.GetOrdinal("UltimaActualizacion"))
        };
    }
}