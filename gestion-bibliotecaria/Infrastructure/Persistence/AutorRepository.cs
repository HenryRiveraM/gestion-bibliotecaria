using System.Data;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Infrastructure.Persistence;

public class AutorRepository : IAutorRepositorio, IRepository<Autor, int>
{
    private readonly string _connectionString;

    public AutorRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public AutorRepository(IConfiguration configuration)
        : this(configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
    {
    }

    private string ConnectionString => _connectionString;

    // ?? M�todos extra (igual que Ejemplar)

    public Dictionary<int, string> ObtenerAutoresActivos()
    {
        var autores = new Dictionary<int, string>();

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        const string query = "SELECT AutorId, Nombres FROM autor WHERE Estado = 1";

        using var command = new MySqlCommand(query, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            autores[reader.GetInt32("AutorId")] = reader.GetString("Nombres");
        }

        return autores;
    }

    public DataTable ObtenerAutoresActivosTabla()
    {
        var dt = new DataTable();

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        const string query = @"SELECT AutorId, Nombres, Apellidos 
                               FROM autor 
                               WHERE Estado = 1 
                               ORDER BY Nombres ASC;";

        using var command = new MySqlCommand(query, connection);
        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dt);

        return dt;
    }

    public bool ExisteAutorActivo(int autorId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        const string query = "SELECT COUNT(1) FROM autor WHERE AutorId = @AutorId AND Estado = 1";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@AutorId", autorId);

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result) > 0;
    }

    // ?? M�todos base (igual patr�n que Ejemplar)

    public DataTable Select() => GetAll();

    public void Create(Autor autor) => Insert(autor);

    public void DeleteById(int id)
    {
        Delete(new Autor { AutorId = id });
    }

    public DataTable GetAll()
    {
        DataTable dt = new DataTable();

        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();

            string query = @"SELECT 
                                 AutorId,
                                 UsuarioSesionId,
                                 Nombres,
                                 Apellidos,
                                 Nacionalidad,
                                FechaNacimiento,
                                Estado
                             FROM autor
                             ORDER BY Nombres ASC;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
            {
                adapter.Fill(dt);
            }
        }

        return dt;
    }

    public void Insert(Autor autor)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();

            string query = @"INSERT INTO autor
                (UsuarioSesionId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro)
                VALUES
                (@UsuarioSesionId, UPPER(@Nombres), UPPER(@Apellidos), @Nacionalidad, @FechaNacimiento, @Estado, NOW());";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UsuarioSesionId", autor.UsuarioSesionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nombres", autor.Nombres);
                command.Parameters.AddWithValue("@Apellidos", autor.Apellidos ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FechaNacimiento", autor.FechaNacimiento ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estado", autor.Estado);

                command.ExecuteNonQuery();
            }
        }
    }

    public void Update(Autor autor)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
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

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AutorId", autor.AutorId);
                command.Parameters.AddWithValue("@UsuarioSesionId", autor.UsuarioSesionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nombres", autor.Nombres);
                command.Parameters.AddWithValue("@Apellidos", autor.Apellidos ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FechaNacimiento", autor.FechaNacimiento ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estado", autor.Estado);

                command.ExecuteNonQuery();
            }
        }
    }

    public void Delete(Autor autor)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();

            string query = @"UPDATE autor 
                             SET Estado = 0, UsuarioSesionId = @UsuarioSesionId, UltimaActualizacion = NOW() 
                             WHERE AutorId = @AutorId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AutorId", autor.AutorId);
                command.Parameters.AddWithValue("@UsuarioSesionId", autor.UsuarioSesionId ?? (object)DBNull.Value);
                command.ExecuteNonQuery();
            }
        }
    }

    public Autor? GetById(int id)
    {
        Autor? autor = null;

        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();

            string query = "SELECT * FROM autor WHERE AutorId = @AutorId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AutorId", id);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        autor = new Autor
                        {
                            AutorId = reader.GetInt32("AutorId"),
                            UsuarioSesionId = reader.IsDBNull("UsuarioSesionId") ? null : reader.GetInt32("UsuarioSesionId"),
                            Nombres = reader.GetString("Nombres"),
                            Apellidos = reader.IsDBNull("Apellidos") ? null : reader.GetString("Apellidos"),
                            Nacionalidad = reader.IsDBNull("Nacionalidad") ? null : reader.GetString("Nacionalidad"),
                            FechaNacimiento = reader.IsDBNull("FechaNacimiento") ? null : reader.GetDateTime("FechaNacimiento"),
                            Estado = reader.GetBoolean("Estado"),
                            FechaRegistro = reader.GetDateTime("FechaRegistro"),
                            UltimaActualizacion = reader.IsDBNull("UltimaActualizacion") ? null : reader.GetDateTime("UltimaActualizacion")
                        };
                    }
                }
            }
        }

        return autor;
    }
}
