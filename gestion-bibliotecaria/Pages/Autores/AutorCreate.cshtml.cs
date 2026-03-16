using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.FactoryCreators;

namespace gestion_bibliotecaria.Pages;

public class AutorCreateModel : PageModel
{
    private readonly RepositoryFactory<Autor> _autorRepositoryFactory;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor();

    public AutorCreateModel(RepositoryFactory<Autor> autorRepositoryFactory)
    {
        _autorRepositoryFactory = autorRepositoryFactory;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost()
    {
        Autor.Nombres = ValidadorEntrada.NormalizarEspacios(Autor.Nombres);
        Autor.Apellidos = ValidadorEntrada.NormalizarEspacios(Autor.Apellidos);
        Autor.Nacionalidad = ValidadorEntrada.NormalizarEspacios(Autor.Nacionalidad);

        if (ValidadorEntrada.EstaVacio(Autor.Nombres))
        {
            ModelState.AddModelError("Autor.Nombres", "Los nombres son obligatorios.");
        }
        else
        {
            if (!ValidadorEntrada.SoloLetras(Autor.Nombres))
            {
                ModelState.AddModelError("Autor.Nombres", "Los nombres solo pueden contener letras y espacios.");
            }
            else if (ValidadorEntrada.ExcedeLongitud(Autor.Nombres, 100))
            {
                ModelState.AddModelError("Autor.Nombres", "Los nombres exceden la longitud máxima de 100 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Autor.Apellidos))
        {
            if (!ValidadorEntrada.SoloLetras(Autor.Apellidos))
            {
                ModelState.AddModelError("Autor.Apellidos", "Los apellidos solo pueden contener letras y espacios.");
            }
            else if (ValidadorEntrada.ExcedeLongitud(Autor.Apellidos, 100))
            {
                ModelState.AddModelError("Autor.Apellidos", "Los apellidos exceden la longitud máxima de 100 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Autor.Nacionalidad))
        {
            if (!ValidadorEntrada.SoloLetras(Autor.Nacionalidad))
            {
                ModelState.AddModelError("Autor.Nacionalidad", "La nacionalidad solo puede contener letras y espacios.");
            }
            else if (ValidadorEntrada.ExcedeLongitud(Autor.Nacionalidad, 100))
            {
                ModelState.AddModelError("Autor.Nacionalidad", "La nacionalidad excede la longitud máxima de 100 caracteres.");
            }
        }

        if (!ValidadorEntrada.FechaNoFutura(Autor.FechaNacimiento))
        {
            ModelState.AddModelError("Autor.FechaNacimiento", "La fecha de nacimiento no puede ser futura.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var repository = _autorRepositoryFactory.CreateRepository();
        repository.Insert(Autor);

        return RedirectToPage("Autor");
    }
}