using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Domain.Ports;
using System;
using System.Linq;

namespace gestion_bibliotecaria.Aplicacion.Fachadas;

public class AnulacionFachada : IAnulacionFachada
{
    private readonly IPrestamoServicio _prestamoServicio;
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly IDetalleRepositorio _detalleRepositorio;
    private readonly IEjemplarDisponibilidadFachada _disponibilidadFachada;
    public AnulacionFachada(
        IPrestamoServicio prestamoServicio,
        IDetalleRepositorio detalleRepositorio,
        IEjemplarServicio ejemplarServicio,
        IEjemplarDisponibilidadFachada disponibilidadFachada)
    {
        _prestamoServicio = prestamoServicio;
        _detalleRepositorio = detalleRepositorio;
        _ejemplarServicio = ejemplarServicio;
        _disponibilidadFachada = disponibilidadFachada;
    }

    public Result AnularPrestamo(int prestamoId, int? usuarioSesionId = null, string? motivo = null)
    {
        if (prestamoId <= 0)
            return Result.Failure(new Error("Anulacion.Error", "El id del préstamo no es válido."));

        try
        {
            var prestamo = _prestamoServicio.GetById(prestamoId);
            if (prestamo == null)
                return Result.Failure(new Error("Anulacion.NotFound", "Préstamo no encontrado."));

            // Solo se anula si sigue activo/recién creado
            if (prestamo.Estado != 1)
                return Result.Failure(new Error("Anulacion.Error", "Solo se pueden anular préstamos activos o recién creados."));

            // Si ya hubo devolución real, ya no entra en este caso de uso
            if (prestamo.FechaDevolucionReal != null)
                return Result.Failure(new Error("Anulacion.Error", "No se puede anular un préstamo que ya registra devolución."));

            var detalles = _detalleRepositorio.GetAll()
                .Where(d => d.PrestamoId == prestamoId)
                .ToList();

            if (!detalles.Any())
                return Result.Failure(new Error("Anulacion.Error", "El préstamo no tiene detalles asociados."));

            foreach (var detalle in detalles)
            {
                if (detalle.EstadoDetalle == 2)
                    return Result.Failure(new Error("Anulacion.Error", "No se puede anular porque uno de los ejemplares ya fue devuelto."));

                detalle.EstadoDetalle = 0; // ANULADO
                detalle.ObservacionesSalida = motivo;
                detalle.UsuarioSesionId = usuarioSesionId;
                detalle.UltimaActualizacion = DateTime.Now;

                _detalleRepositorio.Update(detalle);

                var result = _disponibilidadFachada.CambiarDisponibilidad(detalle.EjemplarId, true, usuarioSesionId);
                if (result.IsFailure)
                    return Result.Failure(result.Error);
            }

            prestamo.Estado = 0; // ANULADO
            prestamo.ObservacionesSalida = motivo;
            prestamo.UsuarioSesionId = usuarioSesionId;
            prestamo.UltimaActualizacion = DateTime.Now;

            var dto = new gestion_bibliotecaria.Aplicacion.Dtos.PrestamoDto
            {
                PrestamoId = prestamo.PrestamoId,
                LectorId = prestamo.LectorId,
                FechaPrestamo = prestamo.FechaPrestamo,
                FechaDevolucionEsperada = prestamo.FechaDevolucionEsperada,
                FechaDevolucionReal = prestamo.FechaDevolucionReal,
                ObservacionesSalida = prestamo.ObservacionesSalida,
                ObservacionesEntrada = prestamo.ObservacionesEntrada,
                Estado = prestamo.Estado,
                UsuarioSesionId = prestamo.UsuarioSesionId
            };

            var resultado = _prestamoServicio.Update(dto);
            if (resultado.IsFailure)
                return Result.Failure(resultado.Error);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Anulacion.Error", $"Error al anular préstamo: {ex.Message}"));
        }
    }
}