using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Aplicacion.Interfaces;
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

    public void OnGet()
    {
        Libro.Estado = true;
    }

    public IActionResult OnPost()
    {
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
}
