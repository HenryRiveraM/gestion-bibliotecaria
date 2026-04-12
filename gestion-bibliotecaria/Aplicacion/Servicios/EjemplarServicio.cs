using System.Data;
using System.Text.RegularExpressions;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Validations;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public class EjemplarServicio : IEjemplarServicio
{
    private readonly IEjemplarRepositorio _ejemplarRepositorio;

    public EjemplarServicio(IEjemplarRepositorio ejemplarRepositorio)
    {
        _ejemplarRepositorio = ejemplarRepositorio;
    }

    public DataTable Select()
    {
        var dt = new DataTable();
        dt.Columns.Add("EjemplarId", typeof(int));
        dt.Columns.Add("UsuarioSesionId", typeof(int));
        dt.Columns.Add("LibroId", typeof(int));
        dt.Columns.Add("LibroTitulo", typeof(string));
        dt.Columns.Add("CodigoInventario", typeof(string));
        dt.Columns.Add("EstadoConservacion", typeof(string));
        dt.Columns.Add("Disponible", typeof(bool));
        dt.Columns.Add("DadoDeBaja", typeof(bool));
        dt.Columns.Add("MotivoBaja", typeof(string));
        dt.Columns.Add("Ubicacion", typeof(string));
        dt.Columns.Add("Estado", typeof(bool));

        var ejemplares = _ejemplarRepositorio.GetAll();
        foreach (var e in ejemplares)
        {
            dt.Rows.Add(
                e.EjemplarId,
                e.UsuarioSesionId ?? (object)DBNull.Value,
                e.LibroId,
                e.LibroTitulo ?? (object)DBNull.Value,
                e.CodigoInventario,
                e.EstadoConservacion ?? (object)DBNull.Value,
                e.Disponible,
                e.DadoDeBaja,
                e.MotivoBaja ?? (object)DBNull.Value,
                e.Ubicacion ?? (object)DBNull.Value,
                e.Estado
            );
        }
        return dt;
    }

    public void Create(Ejemplar ejemplar) => _ejemplarRepositorio.Insert(ejemplar);

    public void Update(Ejemplar ejemplar) => _ejemplarRepositorio.Update(ejemplar);

    public void Delete(Ejemplar ejemplar) => _ejemplarRepositorio.Delete(ejemplar);

    public Ejemplar? GetById(int id) => _ejemplarRepositorio.GetById(id);

    public Dictionary<int, string> ObtenerTitulosLibros() => _ejemplarRepositorio.ObtenerTitulosLibros();

    public DataTable ObtenerLibrosActivos()
    {
        var dt = new DataTable();
        dt.Columns.Add("LibroId", typeof(int));
        dt.Columns.Add("Titulo", typeof(string));
        dt.Columns.Add("Editorial", typeof(string));
        
        var libros = _ejemplarRepositorio.ObtenerLibrosActivos();
        foreach (var l in libros)
        {
            dt.Rows.Add(
                l.LibroId,
                l.Titulo,
                l.Editorial ?? (object)DBNull.Value
            );
        }
        return dt;
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