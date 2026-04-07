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

    public DataTable Select() => _ejemplarRepositorio.GetAll();

    public void Create(Ejemplar ejemplar) => _ejemplarRepositorio.Insert(ejemplar);

    public void Update(Ejemplar ejemplar) => _ejemplarRepositorio.Update(ejemplar);

    public void Delete(Ejemplar ejemplar) => _ejemplarRepositorio.Delete(ejemplar);

    public Ejemplar? GetById(int id) => _ejemplarRepositorio.GetById(id);

    public Dictionary<int, string> ObtenerTitulosLibros() => _ejemplarRepositorio.ObtenerTitulosLibros();

    public DataTable ObtenerLibrosActivos() => _ejemplarRepositorio.ObtenerLibrosActivos();

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