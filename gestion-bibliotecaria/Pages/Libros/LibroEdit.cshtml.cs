using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;

namespace gestion_bibliotecaria.Pages;

public class LibroEditModel : PageModel
{
    private readonly RepositoryFactory<Libro, int> _libroRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;

    public LibroEditModel(
        RepositoryFactory<Libro, int> libroRepositoryFactory,
        RouteTokenService routeTokenService)
    {
        _libroRepositoryFactory = libroRepositoryFactory;
        _routeTokenService = routeTokenService;
    }

    [BindProperty]
    public int LibroId { get; set; }

    [BindProperty]
    public string LibroToken { get; set; } = string.Empty;

    [BindProperty]
    public int AutorId { get; set; }

    [BindProperty]
    public string Titulo { get; set; } = string.Empty;

    [BindProperty]
    public string? Editorial { get; set; }

    [BindProperty]
    public string? Edicion { get; set; }

    [BindProperty]
    public int? AñoPublicacion { get; set; }

    [BindProperty]
    public string? Descripcion { get; set; }

    [BindProperty]
    public bool Estado { get; set; }

    [BindProperty]
    public DateTime FechaRegistro { get; set; }

    public DataTable Autores { get; set; } = new();

    public IActionResult OnGet(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        if (!CargarPagina(id))
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!_routeTokenService.TryObtenerId(LibroToken, out var id))
        {
            return NotFound();
        }

        LibroId = id;

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
            CargarAutores();
            return Page();
        }

        var repository = _libroRepositoryFactory.CreateRepository();

        var libro = new Libro
        {
            LibroId = LibroId,
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

        return Redirect("/Libro");
    }

    private bool CargarPagina(int id)
    {
        var repository = _libroRepositoryFactory.CreateRepository();
        var libro = repository.GetById(id);

        if (libro == null)
        {
            return false;
        }

        LibroId = libro.LibroId;
        LibroToken = _routeTokenService.CrearToken(LibroId);
        AutorId = libro.AutorId;
        Titulo = libro.Titulo;
        Editorial = libro.Editorial;
        Edicion = libro.Edicion;
        AñoPublicacion = libro.AñoPublicacion;
        Descripcion = libro.Descripcion;
        Estado = libro.Estado;
        FechaRegistro = libro.FechaRegistro;

        CargarAutores();
        return true;
    }

    private void CargarAutores()
    {
        var repository = _libroRepositoryFactory.CreateRepository();

        if (repository is LibroRepository libroRepository)
        {
            Autores = libroRepository.ObtenerAutoresActivos();
        }
    }
}