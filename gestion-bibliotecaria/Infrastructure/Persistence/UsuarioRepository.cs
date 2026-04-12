using System.Data;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace gestion_bibliotecaria.Infrastructure.Persistence;

public class UsuarioRepository : IUsuarioRepositorio, IRepository<Usuario, int>
{
    private readonly string _connectionString;

    public UsuarioRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Constructor para inyección de dependencias
    public UsuarioRepository(IConfiguration configuration)
        : this(configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
    {
    }

    private string ConnectionString => _connectionString;

    // --- MÉTODOS DE BÚSQUEDA ---

    public Usuario? GetByCi(string ci)
    {
        Usuario? usuario = null;
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            string query = "SELECT * FROM usuario WHERE CI = @CI LIMIT 1;";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CI", ci);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = MapReaderToUsuario(reader);
                    }
                }
            }
        }
        return usuario;
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
                        usuario = MapReaderToUsuario(reader);
                    }
                }
            }
        }
        return usuario;
    }

    public Usuario? GetByNombreUsuario(string nombreUsuario)
    {
        Usuario? usuario = null;
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            string query = "SELECT * FROM usuario WHERE NombreUsuario = @NombreUsuario LIMIT 1;";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = MapReaderToUsuario(reader);
                    }
                }
            }
        }
        return usuario;
    }

    // --- MAPPING (EL SECRETO PARA EVITAR ERRORES DE NULO) ---
    private Usuario MapReaderToUsuario(MySqlDataReader reader)
    {
        return new Usuario
        {
            UsuarioId = reader.GetInt32("UsuarioId"),
            UsuarioSesionId = reader.IsDBNull("UsuarioSesionId") ? null : reader.GetInt32("UsuarioSesionId"),
            Nombres = reader.GetString("Nombres"),
            PrimerApellido = reader.GetString("PrimerApellido"),
            // Manejo de nulos para campos opcionales
            SegundoApellido = reader.IsDBNull("SegundoApellido") ? null : reader.GetString("SegundoApellido"),
            Email = reader.GetString("Email"),
            NombreUsuario = reader.GetString("NombreUsuario"),
            PasswordHash = reader.GetString("PasswordHash"),
            Salt = reader.IsDBNull("Salt") ? null : reader.GetString("Salt"),
            Rol = reader.GetString("Rol"),
            Estado = reader.GetBoolean("Estado"),
            FechaRegistro = reader.GetDateTime("FechaRegistro"),
            UltimaActualizacion = reader.IsDBNull("UltimaActualizacion") ? null : reader.GetDateTime("UltimaActualizacion"),
            CI = reader.IsDBNull("CI") ? null : reader.GetString("CI")
        };
    }

    // --- MÉTODOS DE ESCRITURA ---

    public void Insert(Usuario usuario)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            string query = @"INSERT INTO usuario
                (UsuarioSesionId, Nombres, PrimerApellido, SegundoApellido, Email, NombreUsuario, PasswordHash, Salt, Rol, Estado, FechaRegistro, CI)
                VALUES
                (@UsuarioSesionId, UPPER(@Nombres), UPPER(@PrimerApellido), UPPER(@SegundoApellido), @Email, @NombreUsuario, @PasswordHash, @Salt, @Rol, @Estado, NOW(), @CI);";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UsuarioSesionId", usuario.UsuarioSesionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nombres", usuario.Nombres);
                command.Parameters.AddWithValue("@PrimerApellido", usuario.PrimerApellido);
                command.Parameters.AddWithValue("@SegundoApellido", usuario.SegundoApellido ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", usuario.Email);
                command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
                command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
                command.Parameters.AddWithValue("@Salt", usuario.Salt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Rol", usuario.Rol);
                command.Parameters.AddWithValue("@Estado", usuario.Estado);
                command.Parameters.AddWithValue("@CI", usuario.CI ?? (object)DBNull.Value);

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
                    Nombres = UPPER(@Nombres),
                    PrimerApellido = UPPER(@PrimerApellido),
                    SegundoApellido = UPPER(@SegundoApellido),
                    Email = @Email,
                    NombreUsuario = @NombreUsuario,
                    PasswordHash = @PasswordHash,
                    Salt = @Salt,
                    Rol = @Rol,
                    Estado = @Estado,
                    CI = @CI,
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
                command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
                command.Parameters.AddWithValue("@Salt", usuario.Salt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Rol", usuario.Rol);
                command.Parameters.AddWithValue("@Estado", usuario.Estado);
                command.Parameters.AddWithValue("@CI", usuario.CI ?? (object)DBNull.Value);

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
                             SET UsuarioSesionId = @UsuarioSesionId,
                                 Estado = 0,
                                 UltimaActualizacion = NOW()
                             WHERE UsuarioId = @UsuarioId;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UsuarioSesionId", usuario.UsuarioSesionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UsuarioId", usuario.UsuarioId);
                command.ExecuteNonQuery();
            }
        }
    }

    // --- MÉTODOS DE UTILIDAD Y COMPATIBILIDAD ---

    public DataTable GetAll()
    {
        DataTable dt = new DataTable();
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            string query = "SELECT * FROM usuario ORDER BY Nombres ASC;";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
            {
                adapter.Fill(dt);
            }
        }
        return dt;
    }

    public DataTable Select() => GetAll();
    public void Create(Usuario usuario) => Insert(usuario);
    public void DeleteById(int id) => Delete(new Usuario { UsuarioId = id });

    public string JoinCiComp(string ci, string complemento)
    {
        if (string.IsNullOrWhiteSpace(complemento)) return ci;
        return $"{ci}-{complemento}";
    }

    public bool ExisteNombreUsuario(string nombreUsuario)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            string query = "SELECT COUNT(1) FROM usuario WHERE NombreUsuario = @NombreUsuario;";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }
    }

    public bool ExisteEmail(string email)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            string query = "SELECT COUNT(1) FROM usuario WHERE Email = @Email;";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }
    }
}