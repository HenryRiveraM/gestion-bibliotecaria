using gestion_bibliotecaria.Aplicacion.Dtos;
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

    public IEnumerable<LibroDto> Select()
    {
        var libros = _libroRepositorio.GetAll();
        var autores = _libroRepositorio.ObtenerNombresAutores().ToDictionary(a => a.AutorId, a => $"{a.Nombres} {(a.Apellidos ?? "")}".Trim());

        return libros.Select(l => new LibroDto
        {
            LibroId = l.LibroId,
            UsuarioSesionId = l.UsuarioSesionId,
            AutorId = l.AutorId,
            NombreAutor = autores.TryGetValue(l.AutorId, out var nombre) ? nombre : string.Empty,
            Titulo = l.Titulo,
            ISBN = l.ISBN,
            Editorial = l.Editorial,
            Genero = l.Genero,
            Edicion = l.Edicion,
            AñoPublicacion = l.AñoPublicacion,
            NumeroPaginas = l.NumeroPaginas,
            Idioma = l.Idioma,
            PaisPublicacion = l.PaisPublicacion,
            Descripcion = l.Descripcion,
            Estado = l.Estado
        });
    }

    public LibroDto? GetById(int id)
    {
        var l = _libroRepositorio.GetById(id);
        if (l == null) return null;

        var autores = _libroRepositorio.ObtenerNombresAutores().ToDictionary(a => a.AutorId, a => $"{a.Nombres} {(a.Apellidos ?? "")}".Trim());

        return new LibroDto
        {
            LibroId = l.LibroId,
            UsuarioSesionId = l.UsuarioSesionId,
            AutorId = l.AutorId,
            NombreAutor = autores.TryGetValue(l.AutorId, out var nombre) ? nombre : string.Empty,
            Titulo = l.Titulo,
            ISBN = l.ISBN,
            Editorial = l.Editorial,
            Genero = l.Genero,
            Edicion = l.Edicion,
            AñoPublicacion = l.AñoPublicacion,
            NumeroPaginas = l.NumeroPaginas,
            Idioma = l.Idioma,
            PaisPublicacion = l.PaisPublicacion,
            Descripcion = l.Descripcion,
            Estado = l.Estado
        };
    }

    public Result Create(LibroDto dto, string? nombreAutorNuevo)
    {
        var libro = new Libro
        {
            UsuarioSesionId = dto.UsuarioSesionId,
            AutorId = dto.AutorId,
            Titulo = dto.Titulo,
            ISBN = dto.ISBN,
            Editorial = dto.Editorial,
            Genero = dto.Genero,
            Edicion = dto.Edicion,
            AñoPublicacion = dto.AñoPublicacion,
            NumeroPaginas = dto.NumeroPaginas,
            Idioma = dto.Idioma,
            PaisPublicacion = dto.PaisPublicacion,
            Descripcion = dto.Descripcion,
            Estado = dto.Estado,
            FechaRegistro = DateTime.Now
        };

        var nombreAutorNormalizado = ValidadorEntrada.NormalizarEspacios(nombreAutorNuevo);
        var validationResult = ValidarLibro(libro, nombreAutorNormalizado);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (libro.AutorId != 0 && !ExisteAutorActivo(libro.AutorId))
        {
            return Result.Failure(new Error("Libro.AutorId", "El autor seleccionado está inactivo o no existe."));
        }

        if (libro.AutorId == 0 && !string.IsNullOrWhiteSpace(nombreAutorNormalizado))
        {
            libro.AutorId = _libroRepositorio.InsertarAutorYObtenerID(nombreAutorNormalizado, libro.UsuarioSesionId);
        }

        _libroRepositorio.Insert(libro);
        return Result.Success();
    }

    public Result Update(LibroDto dto)
    {
        var libroExistente = _libroRepositorio.GetById(dto.LibroId);
        if (libroExistente == null) return Result.Failure(new Error("Libro.NoEncontrado", "El libro no existe o fue eliminado."));

        var libro = new Libro
        {
            LibroId = dto.LibroId,
            UsuarioSesionId = dto.UsuarioSesionId ?? libroExistente.UsuarioSesionId,
            AutorId = dto.AutorId,
            Titulo = dto.Titulo,
            ISBN = dto.ISBN,
            Editorial = dto.Editorial,
            Genero = dto.Genero,
            Edicion = dto.Edicion,
            AñoPublicacion = dto.AñoPublicacion,
            NumeroPaginas = dto.NumeroPaginas,
            Idioma = dto.Idioma,
            PaisPublicacion = dto.PaisPublicacion,
            Descripcion = dto.Descripcion,
            Estado = dto.Estado,
            UltimaActualizacion = DateTime.Now
        };

        var validationResult = ValidarLibro(libro, null);
        if (validationResult.IsFailure) return validationResult;

        if (libro.AutorId != 0 && !ExisteAutorActivo(libro.AutorId))
        {
            return Result.Failure(new Error("Libro.AutorId", "El autor seleccionado está inactivo o no existe."));
        }

        _libroRepositorio.Update(libro);
        return Result.Success();
    }

    public Result Delete(int id, int? usuarioSesionId)
    {
        var libro = _libroRepositorio.GetById(id);
        if (libro == null) return Result.Failure(new Error("Libro.NoEncontrado", "El libro no existe o fue eliminado."));

        libro.UsuarioSesionId = usuarioSesionId;
        _libroRepositorio.Delete(libro);
        return Result.Success();
    }

    public Dictionary<int, string> ObtenerNombresAutores()
    {
        var autores = _libroRepositorio.ObtenerNombresAutores();
        return autores.ToDictionary(a => a.AutorId, a => $"{a.Nombres} {(a.Apellidos ?? "")}".Trim());
    }

    public IEnumerable<AutorDto> ObtenerAutoresActivos()
    {
        var autores = _libroRepositorio.ObtenerAutoresActivos();
        return autores.Select(a => new AutorDto
        {
            AutorId = a.AutorId,
            Nombres = a.Nombres,
            Apellidos = a.Apellidos,
            Nacionalidad = a.Nacionalidad
        });
    }

    public bool ExisteAutorActivo(int autorId) => _libroRepositorio.ExisteAutorActivo(autorId);

    public int InsertarAutorYObtenerID(string nombreCompleto, int? usuarioSesionId)
        => _libroRepositorio.InsertarAutorYObtenerID(nombreCompleto, usuarioSesionId);

    private Result ValidarLibro(Libro libro, string? nombreAutorNuevo)
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
