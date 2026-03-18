using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;

namespace gestion_bibliotecaria.Pages;

public class LibroEditModel : PageModel
{
    private readonly ILibroFactory _libroFactory;
    private readonly LibroRepository _repository;
    private readonly RouteTokenService _routeTokenService;

    public LibroEditModel(
        ILibroFactory libroFactory,
        LibroRepository repository,
        RouteTokenService routeTokenService)
    {
        _libroFactory = libroFactory;
        _repository = repository;
        _routeTokenService = routeTokenService;
    }

    [BindProperty] public int LibroId { get; set; }
    [BindProperty] public string LibroToken { get; set; } = string.Empty;
    [BindProperty] public int AutorId { get; set; }
    [BindProperty] public string Titulo { get; set; } = string.Empty;
    [BindProperty] public string? Editorial { get; set; }
    [BindProperty] public string? Edicion { get; set; }
    [BindProperty] public int? AñoPublicacion { get; set; }
    [BindProperty] public string? Descripcion { get; set; }
    [BindProperty] public bool Estado { get; set; }
    [BindProperty] public DateTime FechaRegistro { get; set; }

    public DataTable Autores { get; set; } = new();

    public IActionResult OnGet(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
            return NotFound();

        if (!CargarPagina(id))
            return NotFound();

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!_routeTokenService.TryObtenerId(LibroToken, out var id))
            return NotFound();

        LibroId = id;

        if (!ModelState.IsValid)
        {
            Autores = _repository.ObtenerAutoresActivos();
            return Page();
        }

        var libro = _libroFactory.CreateForUpdate(
            LibroId,
            AutorId,
            Titulo,
            Editorial,
            Edicion,
            AñoPublicacion,
            Descripcion,
            Estado);

        _repository.ActualizarLibro(libro);

        return Redirect("/Libro");
    }

    private bool CargarPagina(int id)
    {
        var libro = _repository.ObtenerLibroPorId(id);
        if (libro == null) return false;

        LibroId = libro.Field<int>("LibroId");
        LibroToken = _routeTokenService.CrearToken(LibroId);
        AutorId = libro.Field<int>("AutorId");
        Titulo = libro.Field<string>("Titulo") ?? string.Empty;
        Editorial = libro.Field<string>("Editorial");
        Edicion = libro.Field<string>("Edicion");
        AñoPublicacion = libro.Field<int?>("AñoPublicacion");
        Descripcion = libro.Field<string>("Descripcion");
        Estado = libro.Field<bool>("Estado");
        FechaRegistro = libro.Field<DateTime>("FechaRegistro");

        Autores = _repository.ObtenerAutoresActivos();
        return true;
    }
}