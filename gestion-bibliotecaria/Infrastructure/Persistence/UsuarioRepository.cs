using System.Data;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Infrastructure.Persistence;

public class UsuarioRepository : IUsuarioRepositorio, IRepository<Usuario, int>
{
    private readonly string _connectionString;

    public UsuarioRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public UsuarioRepository(IConfiguration configuration)
        : this(configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
    {
    }

    private string ConnectionString => _connectionString;

    public DataTable Select() => GetAll();

    public void Create(Usuario usuario) => Insert(usuario);

    public void DeleteById(int id)
    {
        Delete(new Usuario { UsuarioId = id });
    }

    public DataTable GetAll()
    {
        DataTable dt = new DataTable();

        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();

            string query = @"SELECT
                                UsuarioId,
                                UsuarioSesionId,
                                Nombres,
                                PrimerApellido,
                                SegundoApellido,
                                Email,
                                NombreUsuario,
                                Hash,
                                Salt,
                                Rol,
                                Estado,
                                FechaRegistro,
                                UltimaActualizacion
                             FROM usuario
                             ORDER BY Nombres ASC;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
            {
                adapter.Fill(dt);
            }
        }

        return dt;
    }

    public void Insert(Usuario usuario)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();

            string query = @"INSERT INTO usuario
                (UsuarioSesionId, Nombres, PrimerApellido, SegundoApellido, Email, NombreUsuario, Hash, Salt, Rol, Estado, FechaRegistro)
                VALUES
                (@UsuarioSesionId, @Nombres, @PrimerApellido, @SegundoApellido, @Email, @NombreUsuario, @Hash, @Salt, @Rol, @Estado, NOW());";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UsuarioSesionId", usuario.UsuarioSesionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nombres", usuario.Nombres);
                command.Parameters.AddWithValue("@PrimerApellido", usuario.PrimerApellido);
                command.Parameters.AddWithValue("@SegundoApellido", usuario.SegundoApellido ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", usuario.Email);
                command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
                command.Parameters.AddWithValue("@Hash", usuario.Hash);
                command.Parameters.AddWithValue("@Salt", usuario.Salt);
                command.Parameters.AddWithValue("@Rol", usuario.Rol);
                command.Parameters.AddWithValue("@Estado", usuario.Estado);

                command.ExecuteNonQuery();
            }
        }
    }

    public void Update(Usuario usuario)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();

            string query = @"UPDATE usuario
                SET UsuarioSesionId = @UsuarioSesionId,
                    Nombres = @Nombres,
                    PrimerApellido = @PrimerApellido,
                    SegundoApellido = @SegundoApellido,
                    Email = @Email,
                    NombreUsuario = @NombreUsuario,
                    Hash = @Hash,
                    Salt = @Salt,
                    Rol = @Rol,
                    Estado = @Estado,
                    UltimaActualizacion = NOW()
                WHERE UsuarioId = @UsuarioId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UsuarioId", usuario.UsuarioId);
                command.Parameters.AddWithValue("@UsuarioSesionId", usuario.UsuarioSesionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nombres", usuario.Nombres);
                command.Parameters.AddWithValue("@PrimerApellido", usuario.PrimerApellido);
                command.Parameters.AddWithValue("@SegundoApellido", usuario.SegundoApellido ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", usuario.Email);
                command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
                command.Parameters.AddWithValue("@Hash", usuario.Hash);
                command.Parameters.AddWithValue("@Salt", usuario.Salt);
                command.Parameters.AddWithValue("@Rol", usuario.Rol);
                command.Parameters.AddWithValue("@Estado", usuario.Estado);

                command.ExecuteNonQuery();
            }
        }
    }

    public void Delete(Usuario usuario)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();

            string query = @"UPDATE usuario
                             SET Estado = 0, UltimaActualizacion = NOW()
                             WHERE UsuarioId = @UsuarioId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UsuarioId", usuario.UsuarioId);
                command.ExecuteNonQuery();
            }
        }
    }

    public Usuario? GetById(int id)
    {
        Usuario? usuario = null;

        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();

            string query = "SELECT * FROM usuario WHERE UsuarioId = @UsuarioId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UsuarioId", id);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            UsuarioId = reader.GetInt32("UsuarioId"),
                            UsuarioSesionId = reader.IsDBNull("UsuarioSesionId") ? null : reader.GetInt32("UsuarioSesionId"),
                            Nombres = reader.GetString("Nombres"),
                            PrimerApellido = reader.GetString("PrimerApellido"),
                            SegundoApellido = reader.IsDBNull("SegundoApellido") ? null : reader.GetString("SegundoApellido"),
                            Email = reader.GetString("Email"),
                            NombreUsuario = reader.GetString("NombreUsuario"),
                            Hash = reader.GetString("Hash"),
                            Salt = reader.GetString("Salt"),
                            Rol = reader.GetString("Rol"),
                            Estado = reader.GetBoolean("Estado"),
                            FechaRegistro = reader.GetDateTime("FechaRegistro"),
                            UltimaActualizacion = reader.IsDBNull("UltimaActualizacion") ? null : reader.GetDateTime("UltimaActualizacion")
                        };
                    }
                }
            }
        }

        return usuario;
    }
}
