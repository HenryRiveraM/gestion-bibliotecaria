using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_bibliotecaria.Pages.Libros;

public class IndexModel : PageModel
{
    private readonly ILibroServicio _libroServicio;

    public IEnumerable<LibroDto> Libros { get; private set; } = new List<LibroDto>();

    public IndexModel(ILibroServicio libroServicio)
    {
        _libroServicio = libroServicio;
    }

    public void OnGet()
    {
        Libros = _libroServicio.Select();
    }
}
