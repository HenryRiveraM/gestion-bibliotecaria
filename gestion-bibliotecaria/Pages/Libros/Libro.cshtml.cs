using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class LibroModel : PageModel
{
    private readonly RepositoryFactory<Libro, int> _libroRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;

    public DataTable Libros { get; set; } = new DataTable();
    public Dictionary<int, string> AutoresNombres { get; set; } = new();
    public DataTable Autores { get; set; } = new();

    public LibroModel(
        RepositoryFactory<Libro, int> libroRepositoryFactory,
        RouteTokenService routeTokenService)
    {
        _libroRepositoryFactory = libroRepositoryFactory;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        var repository = _libroRepositoryFactory.CreateRepository();
        Libros = repository.GetAll();

        if (!Libros.Columns.Contains("LibroToken"))
        {
            Libros.Columns.Add("LibroToken", typeof(string));
        }

        foreach (DataRow row in Libros.Rows)
        {
            var libroId = row.Field<int>("LibroId");
            row["LibroToken"] = _routeTokenService.CrearToken(libroId);
        }

        if (repository is LibroRepository libroRepository)
        {
            AutoresNombres = libroRepository.ObtenerNombresAutores();
            Autores = libroRepository.ObtenerAutoresActivos();
        }
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var libroId))
        {
            return NotFound();
        }

        var repository = _libroRepositoryFactory.CreateRepository();

        repository.Delete(new Libro
        {
            LibroId = libroId
        });

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        int AutorId,
        string Titulo,
        string? Editorial,
        string? Edicion,
        int? AñoPublicacion,
        string? Descripcion,
        bool Estado,
        DateTime FechaRegistro)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        Titulo = ValidadorEntrada.NormalizarEspacios(Titulo);
        Editorial = ValidadorEntrada.NormalizarEspacios(Editorial);
        Edicion = ValidadorEntrada.NormalizarEspacios(Edicion);
        Descripcion = ValidadorEntrada.NormalizarEspacios(Descripcion);

        if (ValidadorEntrada.EstaVacio(Titulo))
        {
            ModelState.AddModelError("Titulo", "El título es obligatorio.");
        }
        else if (ValidadorEntrada.ExcedeLongitud(Titulo, 100))
        {
            ModelState.AddModelError("Titulo", "El título excede la longitud máxima de 100 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(Editorial) &&
            ValidadorEntrada.ExcedeLongitud(Editorial, 100))
        {
            ModelState.AddModelError("Editorial", "La editorial excede la longitud máxima de 100 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(Edicion) &&
            ValidadorEntrada.ExcedeLongitud(Edicion, 50))
        {
            ModelState.AddModelError("Edicion", "La edición excede la longitud máxima de 50 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(Descripcion) &&
            ValidadorEntrada.ExcedeLongitud(Descripcion, 500))
        {
            ModelState.AddModelError("Descripcion", "La descripción excede la longitud máxima de 500 caracteres.");
        }

        if (!ValidadorEntrada.ValidYear(AñoPublicacion))
        {
            ModelState.AddModelError("AñoPublicacion", "El año de publicación no es válido.");
        }

        if (!ModelState.IsValid)
        {
            OnGet();
            return Page();
        }

        var repository = _libroRepositoryFactory.CreateRepository();

        var libro = new Libro
        {
            LibroId = id,
            AutorId = AutorId,
            Titulo = Titulo,
            Editorial = Editorial,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            Descripcion = Descripcion,
            Estado = Estado,
            FechaRegistro = FechaRegistro,
            UltimaActualizacion = DateTime.Now
        };

        repository.Update(libro);

        return RedirectToPage();
    }

    public IActionResult OnPostCrear(
        int AutorId,
        string Titulo,
        string? Editorial,
        string? Edicion,
        int? AñoPublicacion,
        string? Descripcion,
        bool Estado)
    {
        Titulo = ValidadorEntrada.NormalizarEspacios(Titulo);
        Editorial = ValidadorEntrada.NormalizarEspacios(Editorial);
        Edicion = ValidadorEntrada.NormalizarEspacios(Edicion);
        Descripcion = ValidadorEntrada.NormalizarEspacios(Descripcion);

        if (ValidadorEntrada.EstaVacio(Titulo))
        {
            ModelState.AddModelError("Titulo", "El título es obligatorio.");
        }
        else if (ValidadorEntrada.ExcedeLongitud(Titulo, 100))
        {
            ModelState.AddModelError("Titulo", "El título excede la longitud máxima de 100 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(Editorial) &&
            ValidadorEntrada.ExcedeLongitud(Editorial, 100))
        {
            ModelState.AddModelError("Editorial", "La editorial excede la longitud máxima de 100 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(Edicion) &&
            ValidadorEntrada.ExcedeLongitud(Edicion, 50))
        {
            ModelState.AddModelError("Edicion", "La edición excede la longitud máxima de 50 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(Descripcion) &&
            ValidadorEntrada.ExcedeLongitud(Descripcion, 500))
        {
            ModelState.AddModelError("Descripcion", "La descripción excede la longitud máxima de 500 caracteres.");
        }

        if (!ValidadorEntrada.ValidYear(AñoPublicacion))
        {
            ModelState.AddModelError("AñoPublicacion", "El año de publicación no es válido.");
        }

        if (!ModelState.IsValid)
        {
            OnGet();
            return Page();
        }

        var libro = new Libro
        {
            AutorId = AutorId,
            Titulo = Titulo,
            Editorial = Editorial,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            Descripcion = Descripcion,
            Estado = Estado,
            FechaRegistro = DateTime.Now
        };

        var repository = _libroRepositoryFactory.CreateRepository();
        repository.Insert(libro);

        return RedirectToPage();
    }
}