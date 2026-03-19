using System.Data;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.FactoryProducts;

public class EjemplarRepository : IRepository<Ejemplar,int>
{
    private readonly string _connectionString;

    public EjemplarRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public DataTable GetAll()
    {
        DataTable dt = new DataTable();

        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();

            string query = @"SELECT 
                            e.EjemplarId,
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
                (LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado)
                VALUES
                (@LibroId, @CodigoInventario, @EstadoConservacion, @Disponible, @DadoDeBaja, @MotivoBaja, @Ubicacion, @Estado);";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
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
                SET LibroId = @LibroId,
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

            string query = @"UPDATE ejemplar SET Estado = 0 WHERE EjemplarId = @EjemplarId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EjemplarId", e.EjemplarId);
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