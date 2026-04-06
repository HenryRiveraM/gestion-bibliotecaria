using System.Data;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Domain.Validations;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public class LibroServicio : ILibroServicio
{
    private readonly ILibroRepositorio _libroRepositorio;

    public LibroServicio(ILibroRepositorio libroRepositorio)
    {
        _libroRepositorio = libroRepositorio;
    }

    public DataTable Select() => _libroRepositorio.Select();

    public void Create(Libro libro) => _libroRepositorio.Create(libro);

    public void Update(Libro libro) => _libroRepositorio.Update(libro);

    public void Delete(Libro libro) => _libroRepositorio.Delete(libro);

    public Dictionary<int, string> ObtenerNombresAutores() => _libroRepositorio.ObtenerNombresAutores();

    public DataTable ObtenerAutoresActivos() => _libroRepositorio.ObtenerAutoresActivos();

    public bool ExisteAutorActivo(int autorId) => _libroRepositorio.ExisteAutorActivo(autorId);

    public int InsertarAutorYObtenerID(string nombreCompleto) => _libroRepositorio.InsertarAutorYObtenerID(nombreCompleto);

    public Result ValidarLibro(Libro libro, string? nombreAutorNuevo)
    {
        libro.Titulo = ValidadorEntrada.NormalizarEspacios(libro.Titulo);
        libro.ISBN = ValidadorEntrada.NormalizarEspacios(libro.ISBN);
        libro.Editorial = ValidadorEntrada.NormalizarEspacios(libro.Editorial);
        libro.Genero = ValidadorEntrada.NormalizarEspacios(libro.Genero);
        libro.Edicion = ValidadorEntrada.NormalizarEspacios(libro.Edicion);
        libro.Idioma = ValidadorEntrada.NormalizarEspacios(libro.Idioma);
        libro.PaisPublicacion = ValidadorEntrada.NormalizarEspacios(libro.PaisPublicacion);
        libro.Descripcion = ValidadorEntrada.NormalizarEspacios(libro.Descripcion);

        if (ValidadorEntrada.EstaVacio(libro.Titulo)) return Result.Failure(LibroErrors.TituloObligatorio);
        if (ValidadorEntrada.ExcedeLongitud(libro.Titulo, 200)) return Result.Failure(LibroErrors.TituloLongitud);

        if (!string.IsNullOrWhiteSpace(libro.ISBN))
        {
            if (ValidadorEntrada.ExcedeLongitud(libro.ISBN, 20)) return Result.Failure(LibroErrors.IsbnLongitud);
            if (!ValidadorEntrada.ISBNValido(libro.ISBN)) return Result.Failure(LibroErrors.IsbnInvalido);
        }

        if (!string.IsNullOrWhiteSpace(libro.Editorial) && ValidadorEntrada.ExcedeLongitud(libro.Editorial, 100)) return Result.Failure(LibroErrors.EditorialLongitud);
        if (!string.IsNullOrWhiteSpace(libro.Genero) && ValidadorEntrada.ExcedeLongitud(libro.Genero, 100)) return Result.Failure(LibroErrors.GeneroLongitud);
        if (!string.IsNullOrWhiteSpace(libro.Edicion) && ValidadorEntrada.ExcedeLongitud(libro.Edicion, 50)) return Result.Failure(LibroErrors.EdicionLongitud);
        if (libro.NumeroPaginas.HasValue && libro.NumeroPaginas <= 0) return Result.Failure(LibroErrors.PaginasInvalidas);
        if (!ValidadorEntrada.ValidYear(libro.AñoPublicacion)) return Result.Failure(LibroErrors.AnioInvalido);

        if (!string.IsNullOrWhiteSpace(libro.Idioma))
        {
            if (ValidadorEntrada.ExcedeLongitud(libro.Idioma, 50)) return Result.Failure(LibroErrors.IdiomaLongitud);
            if (!ValidadorEntrada.IdiomaPermitido(libro.Idioma)) return Result.Failure(LibroErrors.IdiomaInvalido);
        }

        if (!string.IsNullOrWhiteSpace(libro.PaisPublicacion) && ValidadorEntrada.ExcedeLongitud(libro.PaisPublicacion, 100)) return Result.Failure(LibroErrors.PaisLongitud);
        if (!string.IsNullOrWhiteSpace(libro.Descripcion) && ValidadorEntrada.ExcedeLongitud(libro.Descripcion, 500)) return Result.Failure(LibroErrors.DescripcionLongitud);

        if (libro.AutorId == 0 && string.IsNullOrWhiteSpace(nombreAutorNuevo)) return Result.Failure(LibroErrors.AutorRequerido);

        return Result.Success();
    }
}