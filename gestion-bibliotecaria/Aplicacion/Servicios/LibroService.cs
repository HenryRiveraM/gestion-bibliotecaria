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

    public DataTable Select()
    {
        var libros = _libroRepositorio.Select();
        var dt = new DataTable();
        dt.Columns.Add("LibroId", typeof(int));
        dt.Columns.Add("UsuarioSesionId", typeof(int));
        dt.Columns.Add("AutorId", typeof(int));
        dt.Columns.Add("Titulo", typeof(string));
        dt.Columns.Add("ISBN", typeof(string));
        dt.Columns.Add("Editorial", typeof(string));
        dt.Columns.Add("Genero", typeof(string));
        dt.Columns.Add("Edicion", typeof(string));
        dt.Columns.Add("AñoPublicacion", typeof(int));
        dt.Columns.Add("NumeroPaginas", typeof(int));
        dt.Columns.Add("Idioma", typeof(string));
        dt.Columns.Add("PaisPublicacion", typeof(string));
        dt.Columns.Add("Descripcion", typeof(string));
        dt.Columns.Add("Estado", typeof(bool));

        foreach (var l in libros)
        {
            dt.Rows.Add(
                l.LibroId,
                l.UsuarioSesionId.HasValue ? (object)l.UsuarioSesionId.Value : DBNull.Value,
                l.AutorId,
                l.Titulo,
                string.IsNullOrEmpty(l.ISBN) ? DBNull.Value : l.ISBN,
                string.IsNullOrEmpty(l.Editorial) ? DBNull.Value : l.Editorial,
                string.IsNullOrEmpty(l.Genero) ? DBNull.Value : l.Genero,
                string.IsNullOrEmpty(l.Edicion) ? DBNull.Value : l.Edicion,
                l.AñoPublicacion.HasValue ? (object)l.AñoPublicacion.Value : DBNull.Value,
                l.NumeroPaginas.HasValue ? (object)l.NumeroPaginas.Value : DBNull.Value,
                string.IsNullOrEmpty(l.Idioma) ? DBNull.Value : l.Idioma,
                string.IsNullOrEmpty(l.PaisPublicacion) ? DBNull.Value : l.PaisPublicacion,
                string.IsNullOrEmpty(l.Descripcion) ? DBNull.Value : l.Descripcion,
                l.Estado
            );
        }

        return dt;
    }

    public Libro? GetById(int id) => _libroRepositorio.GetById(id);

    public void Create(Libro libro) => _libroRepositorio.Create(libro);

    public void Update(Libro libro) => _libroRepositorio.Update(libro);

    public void Delete(Libro libro) => _libroRepositorio.Delete(libro);

    public Dictionary<int, string> ObtenerNombresAutores()
    {
        var autores = _libroRepositorio.ObtenerNombresAutores();
        return autores.ToDictionary(a => a.AutorId, a => $"{a.Nombres} {(a.Apellidos ?? "")}".Trim());
    }

    public DataTable ObtenerAutoresActivos()
    {
        var autores = _libroRepositorio.ObtenerAutoresActivos();
        var dt = new DataTable();
        dt.Columns.Add("AutorId", typeof(int));
        dt.Columns.Add("Nombres", typeof(string));
        dt.Columns.Add("Apellidos", typeof(string));
        dt.Columns.Add("Nacionalidad", typeof(string));

        foreach (var a in autores)
        {
            dt.Rows.Add(
                a.AutorId,
                a.Nombres,
                string.IsNullOrEmpty(a.Apellidos) ? DBNull.Value : a.Apellidos,
                string.IsNullOrEmpty(a.Nacionalidad) ? DBNull.Value : a.Nacionalidad
            );
        }

        return dt;
    }

    public bool ExisteAutorActivo(int autorId) => _libroRepositorio.ExisteAutorActivo(autorId);

    public int InsertarAutorYObtenerID(string nombreCompleto, int? usuarioSesionId)
        => _libroRepositorio.InsertarAutorYObtenerID(nombreCompleto, usuarioSesionId);

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
