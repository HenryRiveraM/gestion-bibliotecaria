using System.Data;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Infrastructure.Persistence;

public class EjemplarRepository : IEjemplarRepositorio, IRepository<Ejemplar, int>
{
    private readonly string _connectionString;

    public EjemplarRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public EjemplarRepository(IConfiguration configuration)
        : this(configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
    {
    }

    private string ConnectionString => _connectionString;

    public Dictionary<int, string> ObtenerTitulosLibros()
    {
        var titulos = new Dictionary<int, string>();

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        const string query = "SELECT LibroId, Titulo FROM libro";

        using var command = new MySqlCommand(query, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            titulos[reader.GetInt32("LibroId")] = reader.GetString("Titulo");
        }

        return titulos;
    }

    public DataTable ObtenerLibrosActivos()
    {
        var dataTable = new DataTable();

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        const string query = "SELECT LibroId, Titulo, Editorial FROM libro WHERE Estado = 1 ORDER BY Titulo";

        using var command = new MySqlCommand(query, connection);
        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dataTable);

        return dataTable;
    }

    public bool ExisteLibroActivo(int libroId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        const string query = "SELECT COUNT(1) FROM libro WHERE LibroId = @LibroId AND Estado = 1";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@LibroId", libroId);

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result) > 0;
    }

    public DataTable Select() => GetAll();

    public void Create(Ejemplar ejemplar) => Insert(ejemplar);

    public void DeleteById(int id)
    {
        Delete(new Ejemplar { EjemplarId = id });
    }

    public DataTable GetAll()
    {
        DataTable dt = new DataTable();

        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();

            string query = @"SELECT 
                            e.EjemplarId,
                            e.UsuarioSesionId,
                            e.LibroId,
                            l.Titulo AS LibroTitulo,
                            e.CodigoInventario,
                            e.EstadoConservacion,
                            e.Disponible,
                            e.DadoDeBaja,
                            e.MotivoBaja,
                            e.Ubicacion,
                            e.Estado
                        FROM ejemplar e
                        INNER JOIN libro l ON e.LibroId = l.LibroId
                        ORDER BY l.Titulo ASC;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
            {
                adapter.Fill(dt);
            }
        }

        return dt;
    }

    public void Insert(Ejemplar e)
    {
        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();

            string query = @"INSERT INTO ejemplar
                (UsuarioSesionId, LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado)
                VALUES
                (@UsuarioSesionId, @LibroId, @CodigoInventario, @EstadoConservacion, @Disponible, @DadoDeBaja, @MotivoBaja, @Ubicacion, @Estado);";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UsuarioSesionId", e.UsuarioSesionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LibroId", e.LibroId);
                command.Parameters.AddWithValue("@CodigoInventario", e.CodigoInventario);
                command.Parameters.AddWithValue("@EstadoConservacion", e.EstadoConservacion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Disponible", e.Disponible);
                command.Parameters.AddWithValue("@DadoDeBaja", e.DadoDeBaja);
                command.Parameters.AddWithValue("@MotivoBaja", e.MotivoBaja ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Ubicacion", e.Ubicacion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estado", e.Estado);

                command.ExecuteNonQuery();
            }
        }
    }

    public void Update(Ejemplar e)
    {
        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();

            string query = @"UPDATE ejemplar
                SET UsuarioSesionId = @UsuarioSesionId,
                    LibroId = @LibroId,
                    CodigoInventario = @CodigoInventario,
                    EstadoConservacion = @EstadoConservacion,
                    Disponible = @Disponible,
                    DadoDeBaja = @DadoDeBaja,
                    MotivoBaja = @MotivoBaja,
                    Ubicacion = @Ubicacion,
                    Estado = @Estado
                WHERE EjemplarId = @EjemplarId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EjemplarId", e.EjemplarId);
                command.Parameters.AddWithValue("@UsuarioSesionId", e.UsuarioSesionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LibroId", e.LibroId);
                command.Parameters.AddWithValue("@CodigoInventario", e.CodigoInventario);
                command.Parameters.AddWithValue("@EstadoConservacion", e.EstadoConservacion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Disponible", e.Disponible);
                command.Parameters.AddWithValue("@DadoDeBaja", e.DadoDeBaja);
                command.Parameters.AddWithValue("@MotivoBaja", e.MotivoBaja ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Ubicacion", e.Ubicacion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estado", e.Estado);

                command.ExecuteNonQuery();
            }
        }
    }

    public void Delete(Ejemplar e)
    {
        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();

            string query = @"UPDATE ejemplar SET Estado = 0, UsuarioSesionId = @UsuarioSesionId WHERE EjemplarId = @EjemplarId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EjemplarId", e.EjemplarId);
                command.Parameters.AddWithValue("@UsuarioSesionId", e.UsuarioSesionId ?? (object)DBNull.Value);
                command.ExecuteNonQuery();
            }
        }
    }

    public Ejemplar? GetById(int id)
    {
        Ejemplar? e = null;

        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();

            string query = @"SELECT 
                            EjemplarId,
                            UsuarioSesionId,
                            LibroId,
                            CodigoInventario,
                            EstadoConservacion,
                            Disponible,
                            DadoDeBaja,
                            MotivoBaja,
                            Ubicacion,
                            Estado,
                            FechaRegistro
                        FROM ejemplar
                        WHERE EjemplarId = @Id;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        e = new Ejemplar
                        {
                            EjemplarId = reader.GetInt32("EjemplarId"),
                            UsuarioSesionId = reader.IsDBNull("UsuarioSesionId") ? null : reader.GetInt32("UsuarioSesionId"),
                            LibroId = reader.GetInt32("LibroId"),
                            CodigoInventario = reader.GetString("CodigoInventario"),
                            EstadoConservacion = reader.IsDBNull("EstadoConservacion") ? null : reader.GetString("EstadoConservacion"),
                            Disponible = reader.GetBoolean("Disponible"),
                            DadoDeBaja = reader.GetBoolean("DadoDeBaja"),
                            MotivoBaja = reader.IsDBNull("MotivoBaja") ? null : reader.GetString("MotivoBaja"),
                            Ubicacion = reader.IsDBNull("Ubicacion") ? null : reader.GetString("Ubicacion"),
                            Estado = reader.GetBoolean("Estado"),
                            FechaRegistro = reader.GetDateTime("FechaRegistro")
                        };
                    }
                }
            }
        }

        return e;
    }
}
