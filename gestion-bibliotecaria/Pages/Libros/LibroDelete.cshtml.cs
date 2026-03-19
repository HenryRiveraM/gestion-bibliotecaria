using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class LibroDeleteModel : PageModel
{
    private readonly RepositoryFactory<Libro, int> _libroRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;

    public int LibroId { get; set; }
    public int AutorId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Editorial { get; set; }
    public string? Edicion { get; set; }
    public int? AñoPublicacion { get; set; }
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }

    public string NombreAutor { get; set; } = string.Empty;

    [BindProperty]
    public string LibroToken { get; set; } = string.Empty;

    public LibroDeleteModel(
        RepositoryFactory<Libro, int> libroRepositoryFactory,
        RouteTokenService routeTokenService)
    {
        _libroRepositoryFactory = libroRepositoryFactory;
        _routeTokenService = routeTokenService;
    }

    public IActionResult OnGet(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        LibroToken = token;

        if (!CargarDetalleLibro(id))
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

        var repository = _libroRepositoryFactory.CreateRepository();
        var libro = repository.GetById(id);

        if (libro == null)
        {
            return NotFound();
        }

        repository.Delete(libro);
        return Redirect("/Libro");
    }

    private bool CargarDetalleLibro(int id)
    {
        var repository = _libroRepositoryFactory.CreateRepository();
        var libro = repository.GetById(id);

        if (libro == null)
        {
            return false;
        }

        LibroId = libro.LibroId;
        AutorId = libro.AutorId;
        Titulo = libro.Titulo;
        Editorial = libro.Editorial;
        Edicion = libro.Edicion;
        AñoPublicacion = libro.AñoPublicacion;
        Descripcion = libro.Descripcion;
        Estado = libro.Estado;
        FechaRegistro = libro.FechaRegistro;
        UltimaActualizacion = libro.UltimaActualizacion;

        if (repository is LibroRepository libroRepository)
        {
            NombreAutor = libroRepository.ObtenerNombreAutor(AutorId);
        }

        return true;
    }
}