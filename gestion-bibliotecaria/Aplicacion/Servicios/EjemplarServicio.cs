using System.Text.RegularExpressions;
using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Domain.Validations;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public class EjemplarServicio : IEjemplarServicio
{
    private readonly IEjemplarRepositorio _ejemplarRepositorio;

    public EjemplarServicio(IEjemplarRepositorio ejemplarRepositorio)
    {
        _ejemplarRepositorio = ejemplarRepositorio;
    }

    public IEnumerable<EjemplarDto> Select()
    {
        var ejemplares = _ejemplarRepositorio.GetAll();
        return ejemplares.Select(e => new EjemplarDto
        {
            EjemplarId = e.EjemplarId,
            UsuarioSesionId = e.UsuarioSesionId,
            LibroId = e.LibroId,
            LibroTitulo = e.LibroTitulo,
            CodigoInventario = e.CodigoInventario,
            EstadoConservacion = e.EstadoConservacion,
            Disponible = e.Disponible,
            DadoDeBaja = e.DadoDeBaja,
            MotivoBaja = e.MotivoBaja,
            Ubicacion = e.Ubicacion,
            Estado = e.Estado
        }).ToList();
    }

    public Result<EjemplarDto> Create(EjemplarDto dto)
    {
        var ejemplar = new Ejemplar
        {
            UsuarioSesionId = dto.UsuarioSesionId,
            LibroId = dto.LibroId,
            CodigoInventario = dto.CodigoInventario,
            EstadoConservacion = dto.EstadoConservacion,
            Disponible = dto.Disponible,
            DadoDeBaja = dto.DadoDeBaja,
            MotivoBaja = dto.MotivoBaja,
            Ubicacion = dto.Ubicacion,
            Estado = dto.Estado,
            FechaRegistro = DateTime.Now
        };

        var validacion = ValidarEjemplar(ejemplar);
        if (!validacion.IsSuccess)
            return Result<EjemplarDto>.Failure(validacion.Error);

        if (!ExisteLibroActivo(ejemplar.LibroId))
            return Result<EjemplarDto>.Failure(EjemplarErrors.LibroInvalido);

        _ejemplarRepositorio.Insert(ejemplar);

        dto.EjemplarId = ejemplar.EjemplarId;
        dto.CodigoInventario = ejemplar.CodigoInventario;
        return Result<EjemplarDto>.Success(dto);
    }

    public Result<EjemplarDto> Update(EjemplarDto dto)
    {
        var ejemplarExistente = _ejemplarRepositorio.GetById(dto.EjemplarId);
        if (ejemplarExistente == null)
            return Result<EjemplarDto>.Failure(EjemplarErrors.ErrorProcesado);

        ejemplarExistente.UsuarioSesionId = dto.UsuarioSesionId;
        ejemplarExistente.LibroId = dto.LibroId;
        ejemplarExistente.CodigoInventario = dto.CodigoInventario;
        ejemplarExistente.EstadoConservacion = dto.EstadoConservacion;
        ejemplarExistente.Disponible = dto.Disponible;
        ejemplarExistente.DadoDeBaja = dto.DadoDeBaja;
        ejemplarExistente.MotivoBaja = dto.MotivoBaja;
        ejemplarExistente.Ubicacion = dto.Ubicacion;
        ejemplarExistente.Estado = dto.Estado;
        ejemplarExistente.UltimaActualizacion = DateTime.Now;

        var validacion = ValidarEjemplar(ejemplarExistente);
        if (!validacion.IsSuccess)
            return Result<EjemplarDto>.Failure(validacion.Error);

        if (!ExisteLibroActivo(ejemplarExistente.LibroId))
            return Result<EjemplarDto>.Failure(EjemplarErrors.LibroInvalido);

        _ejemplarRepositorio.Update(ejemplarExistente);

        dto.CodigoInventario = ejemplarExistente.CodigoInventario;
        return Result<EjemplarDto>.Success(dto);
    }

    public Result Delete(EjemplarDto dto)
    {
        var ejemplar = _ejemplarRepositorio.GetById(dto.EjemplarId);
        if (ejemplar == null)
            return Result.Failure(EjemplarErrors.ErrorProcesado);

        _ejemplarRepositorio.Delete(ejemplar);
        return Result.Success();
    }

    public EjemplarDto? GetById(int id)
    {
        var e = _ejemplarRepositorio.GetById(id);
        if (e == null) return null;

        return new EjemplarDto
        {
            EjemplarId = e.EjemplarId,
            UsuarioSesionId = e.UsuarioSesionId,
            LibroId = e.LibroId,
            LibroTitulo = e.LibroTitulo,
            CodigoInventario = e.CodigoInventario,
            EstadoConservacion = e.EstadoConservacion,
            Disponible = e.Disponible,
            DadoDeBaja = e.DadoDeBaja,
            MotivoBaja = e.MotivoBaja,
            Ubicacion = e.Ubicacion,
            Estado = e.Estado
        };
    }

    public Dictionary<int, string> ObtenerTitulosLibros() => _ejemplarRepositorio.ObtenerTitulosLibros();

    public IEnumerable<LibroDto> ObtenerLibrosActivos()
    {
        var libros = _ejemplarRepositorio.ObtenerLibrosActivos();
        return libros.Select(l => new LibroDto
        {
            LibroId = l.LibroId,
            Titulo = l.Titulo,
            Editorial = l.Editorial
        }).ToList();
    }

    public Dictionary<int, string> ObtenerEjemplaresDisponibles() => _ejemplarRepositorio.ObtenerEjemplaresDisponibles();

    public bool ExisteLibroActivo(int libroId) => _ejemplarRepositorio.ExisteLibroActivo(libroId);

    public Result ValidarEjemplar(Ejemplar ejemplar)
    {
        ejemplar.CodigoInventario = NormalizarCodigoInventario(ValidadorEntrada.NormalizarEspacios(ejemplar.CodigoInventario));
        ejemplar.EstadoConservacion = ValidadorEntrada.NormalizarEspacios(ejemplar.EstadoConservacion);
        ejemplar.Ubicacion = ValidadorEntrada.NormalizarEspacios(ejemplar.Ubicacion);
        ejemplar.MotivoBaja = ValidadorEntrada.NormalizarEspacios(ejemplar.MotivoBaja);

        if (ValidadorEntrada.EstaVacio(ejemplar.CodigoInventario))
            return Result.Failure(EjemplarErrors.CodigoRequerido);

        if (!ValidadorEntrada.CodigoInventarioValido(ejemplar.CodigoInventario))
            return Result.Failure(EjemplarErrors.CodigoFormato);

        if (ValidadorEntrada.ExcedeLongitud(ejemplar.CodigoInventario, 30))
            return Result.Failure(EjemplarErrors.CodigoLongitud);

        return Result.Success();
    }

    public static string NormalizarCodigoInventario(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        input = input.Trim().ToUpperInvariant();

        if (long.TryParse(input, out var correlativoNumerico))
        {
            return $"INV-{correlativoNumerico.ToString("D3")}-{DateTime.Now.Year}";
        }

        var coincidencia = Regex.Match(input, @"^INV-(\d+)-(\d{4})$");
        if (!coincidencia.Success)
            return input;

        if (!long.TryParse(coincidencia.Groups[1].Value, out var correlativo))
            return input;

        var numeroFormateado = correlativo.ToString("D3");
        var anio = coincidencia.Groups[2].Value;

        return $"INV-{numeroFormateado}-{anio}";
    }
}