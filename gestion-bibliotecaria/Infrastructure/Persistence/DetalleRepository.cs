using System.Data;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Configuration;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Infrastructure.Persistence;

public class DetalleRepository : IDetalleRepositorio
{
    public DetalleRepository()
    {
    }

    public IEnumerable<Detalle> GetAll()
    {
        var detalles = new List<Detalle>();

        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"SELECT d.DetalleId, d.PrestamoId, d.EjemplarId, d.EstadoDetalle, d.FechaDevolucionReal, 
                        d.ObservacionesSalida, d.ObservacionesEntrada, d.UsuarioSesionId, d.FechaRegistro, d.UltimaActualizacion
                         FROM detalle d
                         ORDER BY d.FechaRegistro DESC;";

        using MySqlCommand command = new MySqlCommand(query, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            detalles.Add(MapearDetalle(reader));
        }

        return detalles;
    }

    public IEnumerable<Detalle> GetByPrestamoId(int prestamoId)
    {
        var detalles = new List<Detalle>();

        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"SELECT d.DetalleId, d.PrestamoId, d.EjemplarId, d.EstadoDetalle, d.FechaDevolucionReal, 
                        d.ObservacionesSalida, d.ObservacionesEntrada, d.UsuarioSesionId, d.FechaRegistro, d.UltimaActualizacion
                         FROM detalle d
                         WHERE d.PrestamoId = @PrestamoId
                         ORDER BY d.FechaRegistro DESC;";

        using MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@PrestamoId", prestamoId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            detalles.Add(MapearDetalle(reader));
        }

        return detalles;
    }

    public Detalle? GetById(int id)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"SELECT d.DetalleId, d.PrestamoId, d.EjemplarId, d.EstadoDetalle, d.FechaDevolucionReal, 
                        d.ObservacionesSalida, d.ObservacionesEntrada, d.UsuarioSesionId, d.FechaRegistro, d.UltimaActualizacion
                         FROM detalle d
                         WHERE d.DetalleId = @DetalleId LIMIT 1;";

        using MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@DetalleId", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapearDetalle(reader);
        }

        return null;
    }

    public void Insert(Detalle detalle)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"INSERT INTO detalle
            (PrestamoId, EjemplarId, EstadoDetalle, FechaDevolucionReal, ObservacionesSalida, ObservacionesEntrada, UsuarioSesionId, FechaRegistro)
            VALUES
            (@PrestamoId, @EjemplarId, @EstadoDetalle, @FechaDevolucionReal, @ObservacionesSalida, @ObservacionesEntrada, @UsuarioSesionId, NOW());";

        using MySqlCommand command = new MySqlCommand(query, connection);
        AgregarParametros(command, detalle);
        command.ExecuteNonQuery();
    }

    public void InsertMany(IEnumerable<Detalle> detalles)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var detalle in detalles)
            {
                string query = @"INSERT INTO detalle
                    (PrestamoId, EjemplarId, EstadoDetalle, FechaDevolucionReal, ObservacionesSalida, ObservacionesEntrada, UsuarioSesionId, FechaRegistro)
                    VALUES
                    (@PrestamoId, @EjemplarId, @EstadoDetalle, @FechaDevolucionReal, @ObservacionesSalida, @ObservacionesEntrada, @UsuarioSesionId, NOW());";

                using MySqlCommand command = new MySqlCommand(query, connection, transaction);
                AgregarParametros(command, detalle);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Update(Detalle detalle)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"UPDATE detalle
            SET PrestamoId = @PrestamoId,
                EjemplarId = @EjemplarId,
                EstadoDetalle = @EstadoDetalle,
                FechaDevolucionReal = @FechaDevolucionReal,
                ObservacionesSalida = @ObservacionesSalida,
                ObservacionesEntrada = @ObservacionesEntrada,
                UsuarioSesionId = @UsuarioSesionId,
                UltimaActualizacion = NOW()
            WHERE DetalleId = @DetalleId;";

        using MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@DetalleId", detalle.DetalleId);
        AgregarParametros(command, detalle);
        command.ExecuteNonQuery();
    }

    public void Delete(Detalle detalle)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"UPDATE detalle SET EstadoDetalle = 0, UsuarioSesionId = @UsuarioSesionId, UltimaActualizacion = NOW() WHERE DetalleId = @DetalleId;";

        using MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@DetalleId", detalle.DetalleId);
        command.Parameters.AddWithValue("@UsuarioSesionId", detalle.UsuarioSesionId ?? (object)DBNull.Value);
        command.ExecuteNonQuery();
    }

    private void AgregarParametros(MySqlCommand command, Detalle detalle)
    {
        command.Parameters.AddWithValue("@PrestamoId", detalle.PrestamoId);
        command.Parameters.AddWithValue("@EjemplarId", detalle.EjemplarId);
        command.Parameters.AddWithValue("@EstadoDetalle", detalle.EstadoDetalle);
        command.Parameters.AddWithValue("@FechaDevolucionReal", detalle.FechaDevolucionReal ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ObservacionesSalida", detalle.ObservacionesSalida ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ObservacionesEntrada", detalle.ObservacionesEntrada ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UsuarioSesionId", detalle.UsuarioSesionId ?? (object)DBNull.Value);
    }

    private Detalle MapearDetalle(MySqlDataReader reader)
    {
        return new Detalle
        {
            DetalleId = reader.GetInt32("DetalleId"),
            PrestamoId = reader.GetInt32("PrestamoId"),
            EjemplarId = reader.GetInt32("EjemplarId"),
            EstadoDetalle = reader.GetByte("EstadoDetalle"),
            FechaDevolucionReal = reader.IsDBNull("FechaDevolucionReal") ? null : reader.GetDateTime("FechaDevolucionReal"),
            ObservacionesSalida = reader.IsDBNull("ObservacionesSalida") ? null : reader.GetString("ObservacionesSalida"),
            ObservacionesEntrada = reader.IsDBNull("ObservacionesEntrada") ? null : reader.GetString("ObservacionesEntrada"),
            UsuarioSesionId = reader.IsDBNull("UsuarioSesionId") ? null : reader.GetInt32("UsuarioSesionId"),
            FechaRegistro = reader.GetDateTime("FechaRegistro"),
            UltimaActualizacion = reader.IsDBNull("UltimaActualizacion") ? null : reader.GetDateTime("UltimaActualizacion")
        };
    }
}
