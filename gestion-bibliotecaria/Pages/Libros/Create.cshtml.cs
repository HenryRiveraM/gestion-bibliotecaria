using gestion_bibliotecaria.Aplicacion.Servicios;
using gestion_bibliotecaria.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_bibliotecaria.Pages.Libros;

public class CreateModel : PageModel
{
    private readonly LibroServicio _libroServicio;

    [BindProperty]
    public Libro Libro { get; set; } = new();

    public CreateModel(LibroServicio libroServicio)
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

        var validacion = _libroServicio.ValidarLibro(Libro, null);
        if (validacion.IsFailure)
        {
            ModelState.AddModelError(string.Empty, validacion.Error.Message);
            return Page();
        }

        Libro.FechaRegistro = DateTime.Now;
        _libroServicio.Create(Libro);

        return RedirectToPage("Index");
    }
}
