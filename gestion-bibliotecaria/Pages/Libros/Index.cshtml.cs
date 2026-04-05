using System.Data;
using gestion_bibliotecaria.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_bibliotecaria.Pages.Libros;

public class IndexModel : PageModel
{
    private readonly LibroServicio _libroServicio;

    public DataTable Libros { get; private set; } = new();

    public IndexModel(LibroServicio libroServicio)
    {
        _libroServicio = libroServicio;
    }

    public void OnGet()
    {
        Libros = _libroServicio.Select();
    }
}
