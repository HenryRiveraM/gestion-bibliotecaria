using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.Pages;

public class LibroDeleteModel : PageModel
{
    private readonly LibroRepository _repository;
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

    public LibroDeleteModel(LibroRepository repository, RouteTokenService routeTokenService)
    {
        _repository = repository;
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

        _repository.EliminarLibro(id);
        return Redirect("/Libro");
    }

    private bool CargarDetalleLibro(int id)
    {
        var libro = _repository.ObtenerLibroPorId(id);
        if (libro == null)
        {
            return false;
        }

        LibroId = libro.Field<int>("LibroId");
        AutorId = libro.Field<int>("AutorId");
        Titulo = libro.Field<string>("Titulo") ?? string.Empty;
        Editorial = libro.Field<string>("Editorial");
        Edicion = libro.Field<string>("Edicion");
        AñoPublicacion = libro.Field<int?>("AñoPublicacion");
        Descripcion = libro.Field<string>("Descripcion");
        Estado = libro.Field<bool>("Estado");
        FechaRegistro = libro.Field<DateTime>("FechaRegistro");
        UltimaActualizacion = libro.Field<DateTime?>("UltimaActualizacion");

        NombreAutor = _repository.ObtenerNombreAutor(AutorId);
        return true;
    }
}