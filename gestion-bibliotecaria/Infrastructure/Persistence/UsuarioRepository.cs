using System.Data;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using gestion_bibliotecaria.Infrastructure.Configuration;
using System.Collections.Generic;

namespace gestion_bibliotecaria.Infrastructure.Persistence;

public class UsuarioRepository : IUsuarioRepositorio, IRepository<Usuario, int>
{
    public UsuarioRepository()
    {
    }

    
    public UsuarioRepository(IConfiguration configuration)
    {
    }

    

    public Usuario? GetByCi(string ci)
    {
        Usuario? usuario = null;
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
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
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
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
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
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
            NombreUsuario = reader.IsDBNull("NombreUsuario") ? null : reader.GetString("NombreUsuario"),
            PasswordHash = reader.IsDBNull("PasswordHash") ? null : reader.GetString("PasswordHash"),
            Rol = reader.GetString("Rol"),
            Estado = reader.GetBoolean("Estado"),
            FechaRegistro = reader.GetDateTime("FechaRegistro"),
            UltimaActualizacion = reader.IsDBNull("UltimaActualizacion") ? null : reader.GetDateTime("UltimaActualizacion"),
            CI = reader.IsDBNull("CI") ? null : reader.GetString("CI")
        };
    }

   

    public void Insert(Usuario usuario)
    {
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
        {
            connection.Open();
            string query = @"INSERT INTO usuario
                (UsuarioSesionId, Nombres, PrimerApellido, SegundoApellido, Email, NombreUsuario, PasswordHash, Rol, Estado, FechaRegistro, CI)
                VALUES
                (@UsuarioSesionId, UPPER(@Nombres), UPPER(@PrimerApellido), UPPER(@SegundoApellido), @Email, @NombreUsuario, @PasswordHash, @Rol, @Estado, NOW(), @CI);";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UsuarioSesionId", usuario.UsuarioSesionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nombres", usuario.Nombres);
                command.Parameters.AddWithValue("@PrimerApellido", usuario.PrimerApellido);
                command.Parameters.AddWithValue("@SegundoApellido", usuario.SegundoApellido ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", usuario.Email);
                command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Rol", usuario.Rol);
                command.Parameters.AddWithValue("@Estado", usuario.Estado);
                command.Parameters.AddWithValue("@CI", usuario.CI ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
        }
    }

    public void Update(Usuario usuario)
    {
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
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
                command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Rol", usuario.Rol);
                command.Parameters.AddWithValue("@Estado", usuario.Estado);
                command.Parameters.AddWithValue("@CI", usuario.CI ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
        }
    }

    public void Delete(Usuario usuario)
    {
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
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

    

    public IEnumerable<Usuario> GetAll()
    {
        var usuarios = new List<Usuario>();
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
        {
            connection.Open();
            string query = "SELECT * FROM usuario ORDER BY Nombres ASC;";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    usuarios.Add(MapReaderToUsuario(reader));
                }
            }
        }
        return usuarios;
    }

    DataTable IRepository<Usuario, int>.GetAll()
    {
        throw new NotImplementedException("Use IUsuarioRepositorio.GetAll instead");
    }

    public DataTable Select() 
    {
        throw new NotImplementedException("Use GetAll instead");
    }

    public void Create(Usuario usuario) => Insert(usuario);
    public void DeleteById(int id) => Delete(new Usuario { UsuarioId = id });

    public string JoinCiComp(string ci, string complemento)
    {
        if (string.IsNullOrWhiteSpace(complemento)) return ci;
        return $"{ci}-{complemento}";
    }

    public bool ExisteNombreUsuario(string nombreUsuario)
    {
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
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
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
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

    public bool ExisteCi(string ci)
    {
        using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
        {
            connection.Open();
            string query = "SELECT COUNT(1) FROM usuario WHERE CI = @CI;";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CI", ci);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }
    }
}