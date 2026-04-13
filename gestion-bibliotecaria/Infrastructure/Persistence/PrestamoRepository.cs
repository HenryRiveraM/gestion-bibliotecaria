using System.Data;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Configuration;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Infrastructure.Persistence;

public class PrestamoRepository : IPrestamoRepositorio
{
    public PrestamoRepository()
    {
    }

    public IEnumerable<Prestamo> GetAll()
    {
        var prestamos = new List<Prestamo>();

        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"SELECT p.PrestamoId, p.EjemplarId, p.LectorId, p.FechaPrestamo, p.FechaDevolucionEsperada, p.FechaDevolucionReal, p.ObservacionesSalida, p.ObservacionesEntrada, p.Estado, p.UsuarioSesionId, p.FechaRegistro, p.UltimaActualizacion
                         FROM prestamo p
                         ORDER BY p.FechaPrestamo DESC;";

        using MySqlCommand command = new MySqlCommand(query, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            prestamos.Add(new Prestamo
            {
                PrestamoId = reader.GetInt32("PrestamoId"),
                EjemplarId = reader.GetInt32("EjemplarId"),
                LectorId = reader.GetInt32("LectorId"),
                FechaPrestamo = reader.GetDateTime("FechaPrestamo"),
                FechaDevolucionEsperada = reader.GetDateTime("FechaDevolucionEsperada"),
                FechaDevolucionReal = reader.IsDBNull("FechaDevolucionReal") ? null : reader.GetDateTime("FechaDevolucionReal"),
                ObservacionesSalida = reader.IsDBNull("ObservacionesSalida") ? null : reader.GetString("ObservacionesSalida"),
                ObservacionesEntrada = reader.IsDBNull("ObservacionesEntrada") ? null : reader.GetString("ObservacionesEntrada"),
                Estado = reader.GetInt32("Estado"),
                UsuarioSesionId = reader.IsDBNull("UsuarioSesionId") ? null : reader.GetInt32("UsuarioSesionId"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                UltimaActualizacion = reader.IsDBNull("UltimaActualizacion") ? null : reader.GetDateTime("UltimaActualizacion")
            });
        }

        return prestamos;
    }

    public void Insert(Prestamo p)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"INSERT INTO prestamo
            (EjemplarId, LectorId, FechaPrestamo, FechaDevolucionEsperada, FechaDevolucionReal, ObservacionesSalida, ObservacionesEntrada, Estado, UsuarioSesionId, FechaRegistro)
            VALUES
            (@EjemplarId, @LectorId, @FechaPrestamo, @FechaDevolucionEsperada, @FechaDevolucionReal, @ObservacionesSalida, @ObservacionesEntrada, @Estado, @UsuarioSesionId, NOW());";

        using MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@EjemplarId", p.EjemplarId);
        command.Parameters.AddWithValue("@LectorId", p.LectorId);
        command.Parameters.AddWithValue("@FechaPrestamo", p.FechaPrestamo);
        command.Parameters.AddWithValue("@FechaDevolucionEsperada", p.FechaDevolucionEsperada);
        command.Parameters.AddWithValue("@FechaDevolucionReal", p.FechaDevolucionReal ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ObservacionesSalida", p.ObservacionesSalida ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ObservacionesEntrada", p.ObservacionesEntrada ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", p.Estado);
        command.Parameters.AddWithValue("@UsuarioSesionId", p.UsuarioSesionId ?? (object)DBNull.Value);

        command.ExecuteNonQuery();
    }

    public void Update(Prestamo p)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"UPDATE prestamo
            SET EjemplarId = @EjemplarId,
                LectorId = @LectorId,
                FechaPrestamo = @FechaPrestamo,
                FechaDevolucionEsperada = @FechaDevolucionEsperada,
                FechaDevolucionReal = @FechaDevolucionReal,
                ObservacionesSalida = @ObservacionesSalida,
                ObservacionesEntrada = @ObservacionesEntrada,
                Estado = @Estado,
                UsuarioSesionId = @UsuarioSesionId,
                UltimaActualizacion = NOW()
            WHERE PrestamoId = @PrestamoId;";

        using MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@PrestamoId", p.PrestamoId);
        command.Parameters.AddWithValue("@EjemplarId", p.EjemplarId);
        command.Parameters.AddWithValue("@LectorId", p.LectorId);
        command.Parameters.AddWithValue("@FechaPrestamo", p.FechaPrestamo);
        command.Parameters.AddWithValue("@FechaDevolucionEsperada", p.FechaDevolucionEsperada);
        command.Parameters.AddWithValue("@FechaDevolucionReal", p.FechaDevolucionReal ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ObservacionesSalida", p.ObservacionesSalida ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ObservacionesEntrada", p.ObservacionesEntrada ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", p.Estado);
        command.Parameters.AddWithValue("@UsuarioSesionId", p.UsuarioSesionId ?? (object)DBNull.Value);

        command.ExecuteNonQuery();
    }

    public void Delete(Prestamo p)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"UPDATE prestamo SET Estado = 0, UsuarioSesionId = @UsuarioSesionId, UltimaActualizacion = NOW() WHERE PrestamoId = @PrestamoId;";

        using MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@PrestamoId", p.PrestamoId);
        command.Parameters.AddWithValue("@UsuarioSesionId", p.UsuarioSesionId ?? (object)DBNull.Value);
        command.ExecuteNonQuery();
    }

    public Prestamo? GetById(int id)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();

        string query = @"SELECT * FROM prestamo WHERE PrestamoId = @PrestamoId LIMIT 1;";

        using MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@PrestamoId", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Prestamo
            {
                PrestamoId = reader.GetInt32("PrestamoId"),
                EjemplarId = reader.GetInt32("EjemplarId"),
                LectorId = reader.GetInt32("LectorId"),
                FechaPrestamo = reader.GetDateTime("FechaPrestamo"),
                FechaDevolucionEsperada = reader.GetDateTime("FechaDevolucionEsperada"),
                FechaDevolucionReal = reader.IsDBNull("FechaDevolucionReal") ? null : reader.GetDateTime("FechaDevolucionReal"),
                ObservacionesSalida = reader.IsDBNull("ObservacionesSalida") ? null : reader.GetString("ObservacionesSalida"),
                ObservacionesEntrada = reader.IsDBNull("ObservacionesEntrada") ? null : reader.GetString("ObservacionesEntrada"),
                Estado = reader.GetInt32("Estado"),
                UsuarioSesionId = reader.IsDBNull("UsuarioSesionId") ? null : reader.GetInt32("UsuarioSesionId"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                UltimaActualizacion = reader.IsDBNull("UltimaActualizacion") ? null : reader.GetDateTime("UltimaActualizacion")
            };
        }

        return null;
    }

    public void InsertManyWithTransaction(IEnumerable<Prestamo> prestamos)
    {
        using var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();
        
        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var p in prestamos)
            {
                string query = @"INSERT INTO prestamo
        (EjemplarId, LectorId, FechaPrestamo, FechaDevolucionEsperada, FechaDevolucionReal, ObservacionesSalida, ObservacionesEntrada, Estado, UsuarioSesionId, FechaRegistro)
        VALUES
        (@EjemplarId, @LectorId, @FechaPrestamo, @FechaDevolucionEsperada, @FechaDevolucionReal, @ObservacionesSalida, @ObservacionesEntrada, @Estado, @UsuarioSesionId, NOW());";

                using MySqlCommand cmd = new MySqlCommand(query, connection, transaction);
                cmd.Parameters.AddWithValue("@EjemplarId", p.EjemplarId);
                cmd.Parameters.AddWithValue("@LectorId", p.LectorId);
                cmd.Parameters.AddWithValue("@FechaPrestamo", p.FechaPrestamo);
                cmd.Parameters.AddWithValue("@FechaDevolucionEsperada", p.FechaDevolucionEsperada);
                cmd.Parameters.AddWithValue("@FechaDevolucionReal", p.FechaDevolucionReal ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ObservacionesSalida", p.ObservacionesSalida ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ObservacionesEntrada", p.ObservacionesEntrada ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Estado", p.Estado);
                cmd.Parameters.AddWithValue("@UsuarioSesionId", p.UsuarioSesionId ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();

                // Update ejemplar to mark as not available
                string updateEjemplar = @"UPDATE ejemplar SET Disponible = 0, UsuarioSesionId = @UsuarioSesionId WHERE EjemplarId = @EjemplarId;";
                using MySqlCommand cmd2 = new MySqlCommand(updateEjemplar, connection, transaction);
                cmd2.Parameters.AddWithValue("@EjemplarId", p.EjemplarId);
                cmd2.Parameters.AddWithValue("@UsuarioSesionId", p.UsuarioSesionId ?? (object)DBNull.Value);
                cmd2.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            try { transaction.Rollback(); } catch { }
            throw;
        }
    }
}