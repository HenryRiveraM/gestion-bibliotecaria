using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return RedirectToPage("/");
        }

        Libros = _libroServicio.Select();
        return Page();
    }

    private bool EsAdminOBibliotecario()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);

        return string.Equals(rol, Usuario.RolAdmin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, Usuario.RolBibliotecario, StringComparison.OrdinalIgnoreCase);
    }
}
