using System;
using System.Collections.Generic;
using System.Linq;
using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Domain.Validations;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public class PrestamoServicio : IPrestamoServicio
{
    private readonly IPrestamoRepositorio _prestamoRepositorio;
    private readonly IEjemplarRepositorio _ejemplarRepositorio;
    private readonly IUsuarioRepositorio _usuarioRepositorio;

    public PrestamoServicio(IPrestamoRepositorio prestamoRepositorio, IEjemplarRepositorio ejemplarRepositorio, IUsuarioRepositorio usuarioRepositorio)
    {
        _prestamoRepositorio = prestamoRepositorio;
        _ejemplarRepositorio = ejemplarRepositorio;
        _usuarioRepositorio = usuarioRepositorio;
    }

    public IEnumerable<PrestamoDto> Select()
    {
        var prestamos = _prestamoRepositorio.GetAll();
        var dtos = new List<PrestamoDto>();

        foreach (var p in prestamos)
        {
            dtos.Add(new PrestamoDto
            {
                PrestamoId = p.PrestamoId,
                EjemplarId = p.EjemplarId,
                LectorId = p.LectorId,
                FechaPrestamo = p.FechaPrestamo,
                FechaDevolucionEsperada = p.FechaDevolucionEsperada,
                FechaDevolucionReal = p.FechaDevolucionReal,
                ObservacionesSalida = p.ObservacionesSalida,
                ObservacionesEntrada = p.ObservacionesEntrada,
                Estado = p.Estado,
                UsuarioSesionId = p.UsuarioSesionId
            });
        }

        return dtos;
    }

    public Result<PrestamoDto> Create(PrestamoDto dto)
    {
        var prestamo = new Prestamo
        {
            EjemplarId = dto.EjemplarId,
            LectorId = dto.LectorId,
            FechaPrestamo = dto.FechaPrestamo,
            FechaDevolucionEsperada = dto.FechaDevolucionEsperada,
            FechaDevolucionReal = dto.FechaDevolucionReal,
            ObservacionesSalida = dto.ObservacionesSalida,
            ObservacionesEntrada = dto.ObservacionesEntrada,
            Estado = dto.Estado,
            UsuarioSesionId = dto.UsuarioSesionId
        };

        var validacion = ValidarPrestamo(prestamo);
        if (validacion.IsFailure)
            return Result<PrestamoDto>.Failure(validacion.Error);

        _prestamoRepositorio.Insert(prestamo);

        dto.PrestamoId = prestamo.PrestamoId;
        return Result<PrestamoDto>.Success(dto);
    }

    // Create and mark ejemplar as not available
    public void CreateAndMarkEjemplar(Prestamo prestamo)
    {
        _prestamoRepositorio.Insert(prestamo);

        // Intentar marcar ejemplar como no disponible
        var ejemplar = _ejemplarRepositorio.GetById(prestamo.EjemplarId);
        if (ejemplar != null)
        {
            ejemplar.Disponible = false;
            ejemplar.UsuarioSesionId = prestamo.UsuarioSesionId ?? ejemplar.UsuarioSesionId;
            _ejemplarRepositorio.Update(ejemplar);
        }
    }

    public void CreateManyAndMarkEjemplares(IEnumerable<Prestamo> prestamos)
    {
        // Delegate to repository which handles transaction and ejemplar update
        _prestamoRepositorio.InsertManyWithTransaction(prestamos);
    }

    public Result<PrestamoDto> Update(PrestamoDto dto)
    {
        var prestamo = _prestamoRepositorio.GetById(dto.PrestamoId);
        if (prestamo == null)
            return Result<PrestamoDto>.Failure(new Error("Prestamo.NotFound", "Prestamo no encontrado"));

        prestamo.EjemplarId = dto.EjemplarId;
        prestamo.LectorId = dto.LectorId;
        prestamo.FechaPrestamo = dto.FechaPrestamo;
        prestamo.FechaDevolucionEsperada = dto.FechaDevolucionEsperada;
        prestamo.FechaDevolucionReal = dto.FechaDevolucionReal;
        prestamo.ObservacionesSalida = dto.ObservacionesSalida;
        prestamo.ObservacionesEntrada = dto.ObservacionesEntrada;
        prestamo.Estado = dto.Estado;
        prestamo.UsuarioSesionId = dto.UsuarioSesionId;

        // No validamos en update igual que en create, porque el ejemplar puede estar ya no disponible (está prestado)
        // Pero podríamos validar las fechas
        if (prestamo.FechaDevolucionEsperada < prestamo.FechaPrestamo)
            return Result<PrestamoDto>.Failure(PrestamoErrors.FechaDevolucionInvalida);

        _prestamoRepositorio.Update(prestamo);

        return Result<PrestamoDto>.Success(dto);
    }

    public Result Delete(PrestamoDto dto)
    {
        var prestamo = _prestamoRepositorio.GetById(dto.PrestamoId);
        if (prestamo == null)
            return Result.Failure(new Error("Prestamo.NotFound", "Prestamo no encontrado"));

        _prestamoRepositorio.Delete(prestamo);
        return Result.Success();
    }

    public Prestamo? GetById(int id) => _prestamoRepositorio.GetById(id);

    public Result ValidarPrestamo(Prestamo prestamo)
    {
        if (prestamo is null)
            return Result.Failure(PrestamoErrors.DatosObligatorios);

        // Verificar que el ejemplar exista y esté disponible
        var ejemplar = _ejemplarRepositorio.GetById(prestamo.EjemplarId);
        if (ejemplar is null || !ejemplar.Disponible)
            return Result.Failure(PrestamoErrors.EjemplarNoDisponible);

        // Verificar que el lector exista y esté activo
        var lector = _usuarioRepositorio.GetById(prestamo.LectorId);
        if (lector is null || !lector.Estado || !string.Equals(lector.Rol, Usuario.RolLector, StringComparison.Ordinal))
            return Result.Failure(PrestamoErrors.LectorInvalido);

        // Fecha de devolución esperada debe ser mayor o igual a fecha de préstamo
        if (prestamo.FechaDevolucionEsperada < prestamo.FechaPrestamo)
            return Result.Failure(PrestamoErrors.FechaDevolucionInvalida);

        return Result.Success();
    }

    public int CountPrestamosActivos(int lectorId)
    {
        var prestamos = _prestamoRepositorio.GetAll();
        return prestamos.Count(p => p.Estado == 1 && p.LectorId == lectorId);
    }
}
