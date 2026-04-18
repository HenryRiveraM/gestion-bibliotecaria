using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Aplicacion.Factories;

public static class DetalleFactory
{
    /// <summary>
    /// Crea detalles de préstamo a partir de un préstamo y sus ejemplares.
    /// Factory Method para generar múltiples detalles de una transacción de préstamo.
    /// </summary>
    public static IEnumerable<Detalle> CrearDetallesDePrestamoMultiple(int prestamoId, IEnumerable<int> ejemplarIds, int? usuarioSesionId = null)
    {
        var detalles = new List<Detalle>();

        foreach (var ejemplarId in ejemplarIds)
        {
            detalles.Add(new Detalle
            {
                PrestamoId = prestamoId,
                EjemplarId = ejemplarId,
                EstadoDetalle = 1, // PRESTADO
                FechaDevolucionReal = null,
                ObservacionesSalida = null,
                ObservacionesEntrada = null,
                UsuarioSesionId = usuarioSesionId,
                FechaRegistro = DateTime.Now,
                UltimaActualizacion = null
            });
        }

        return detalles;
    }

    /// <summary>
    /// Crea un detalle individual de préstamo.
    /// </summary>
    public static Detalle CrearDetalle(int prestamoId, int ejemplarId, int? usuarioSesionId = null, string? observacionesSalida = null)
    {
        return new Detalle
        {
            PrestamoId = prestamoId,
            EjemplarId = ejemplarId,
            EstadoDetalle = 1, // PRESTADO
            FechaDevolucionReal = null,
            ObservacionesSalida = observacionesSalida,
            ObservacionesEntrada = null,
            UsuarioSesionId = usuarioSesionId,
            FechaRegistro = DateTime.Now,
            UltimaActualizacion = null
        };
    }

    /// <summary>
    /// Crea un detalle con estado de devolución.
    /// </summary>
    public static Detalle CrearDetalleDevuelto(int prestamoId, int ejemplarId, DateTime fechaDevolución, 
        int? usuarioSesionId = null, string? observacionesSalida = null, string? observacionesEntrada = null)
    {
        return new Detalle
        {
            PrestamoId = prestamoId,
            EjemplarId = ejemplarId,
            EstadoDetalle = 2, // DEVUELTO
            FechaDevolucionReal = fechaDevolución,
            ObservacionesSalida = observacionesSalida,
            ObservacionesEntrada = observacionesEntrada,
            UsuarioSesionId = usuarioSesionId,
            FechaRegistro = DateTime.Now,
            UltimaActualizacion = null
        };
    }
}
