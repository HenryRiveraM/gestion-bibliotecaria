using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_bibliotecaria.Pages.Libros;

public class CreateModel : PageModel
{
    private readonly ILibroServicio _libroServicio;

    [BindProperty]
    public LibroDto Libro { get; set; } = new();

    public CreateModel(ILibroServicio libroServicio)
    {
        _libroServicio = libroServicio;
    }

    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return RedirectToPage("/");
        }

        Libro.Estado = true;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!EsAdminOBibliotecario())
        {
            return RedirectToPage("/");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = _libroServicio.Create(Libro, null);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return Page();
        }

        return RedirectToPage("Index");
    }

    private bool EsAdminOBibliotecario()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);

        return string.Equals(rol, Usuario.RolAdmin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, Usuario.RolBibliotecario, StringComparison.OrdinalIgnoreCase);
    }
}
