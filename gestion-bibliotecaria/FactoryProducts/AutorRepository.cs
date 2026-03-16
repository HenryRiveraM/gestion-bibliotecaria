using System.Data;
using gestion_bibliotecaria.Models;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.FactoryProducts;

public class AutorRepository : ILibraryRepository<Autor>
{
    private readonly string _connectionString;

    public AutorRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public DataTable GetAll()
    {
        DataTable dt = new DataTable();
        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            try
            {
                connection.Open();
                string query = "SELECT * FROM autor";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(dt);
                }
            }
            catch(Exception)
            {
                // Handle or rethrow as needed
                throw;
            }
        }
        return dt;
    }

    public void Insert(Autor autor)
    {
        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            string query = @"INSERT INTO autor
                            (Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado, FechaRegistro)
                            VALUES
                            (@Nombres, @Apellidos, @Nacionalidad, @FechaNacimiento, @Estado, NOW());";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
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
        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            string query = @"UPDATE autor
                            SET Nombres = @Nombres,
                                Apellidos = @Apellidos,
                                Nacionalidad = @Nacionalidad,
                                FechaNacimiento = @FechaNacimiento,
                                Estado = @Estado,
                                UltimaActualizacion = NOW()
                            WHERE AutorId = @AutorId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AutorId", autor.AutorId);
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
        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            // Soft delete implementation to match existing logic
            string query = "UPDATE autor SET Estado = 0, UltimaActualizacion = NOW() WHERE AutorId = @AutorId;";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AutorId", autor.AutorId);
                command.ExecuteNonQuery();
            }
        }
    }

    public Autor? GetById(int id)
    {
        Autor? autor = null;
        using (MySqlConnection connection = new MySqlConnection(_connectionString))
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
