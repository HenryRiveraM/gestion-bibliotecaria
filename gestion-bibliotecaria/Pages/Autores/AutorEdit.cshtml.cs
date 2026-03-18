using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryCreators;

namespace gestion_bibliotecaria.Pages;

public class AutorEditModel : PageModel
{
    private readonly RepositoryFactory<Autor,int> _autorRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor();

    [BindProperty]
    public string AutorToken { get; set; } = string.Empty;

    public AutorEditModel(RepositoryFactory<Autor,int> autorRepositoryFactory, RouteTokenService routeTokenService)
    {
        _autorRepositoryFactory = autorRepositoryFactory;
        _routeTokenService = routeTokenService;
    }

    public IActionResult OnGet(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        AutorToken = token;

        var repository = _autorRepositoryFactory.CreateRepository();
        var autor = repository.GetById(id);

        if (autor == null)
        {
            return NotFound();
        }

        Autor = autor;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!_routeTokenService.TryObtenerId(AutorToken, out var autorId))
        {
            return NotFound();
        }

        Autor.AutorId = autorId;

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
        repository.Update(Autor);

        return RedirectToPage("Autor");
    }
}