using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Validaciones;

namespace gestion_bibliotecaria.Pages;

public class AutorModel : PageModel
{
    public DataTable AutorDataTable { get; set; } = new DataTable();

    private readonly RepositoryFactory<Autor, int> _autorRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;

    public AutorModel(
        RepositoryFactory<Autor, int> autorRepositoryFactory,
        RouteTokenService routeTokenService)
    {
        _autorRepositoryFactory = autorRepositoryFactory;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        var repository = _autorRepositoryFactory.CreateRepository();
        AutorDataTable = repository.GetAll();

        if (!AutorDataTable.Columns.Contains("AutorToken"))
        {
            AutorDataTable.Columns.Add("AutorToken", typeof(string));
        }

        foreach (DataRow row in AutorDataTable.Rows)
        {
            var autorId = Convert.ToInt32(row["AutorId"]);
            row["AutorToken"] = _routeTokenService.CrearToken(autorId);
        }
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var autorId))
        {
            return NotFound();
        }

        var repository = _autorRepositoryFactory.CreateRepository();

        repository.Delete(new Autor
        {
            AutorId = autorId
        });

        return RedirectToPage();
    }

    public IActionResult OnPostCrear(Autor Autor)
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
            OnGet();
            return Page();
        }

        var repository = _autorRepositoryFactory.CreateRepository();
        repository.Insert(Autor);

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        string Nombres,
        string? Apellidos,
        string? Nacionalidad,
        DateTime? FechaNacimiento,
        bool Estado)
    {
        if (!_routeTokenService.TryObtenerId(token, out var autorId))
        {
            return NotFound();
        }

        var autor = new Autor
        {
            AutorId = autorId,
            Nombres = ValidadorEntrada.NormalizarEspacios(Nombres),
            Apellidos = ValidadorEntrada.NormalizarEspacios(Apellidos),
            Nacionalidad = ValidadorEntrada.NormalizarEspacios(Nacionalidad),
            FechaNacimiento = FechaNacimiento,
            Estado = Estado
        };

        if (ValidadorEntrada.EstaVacio(autor.Nombres))
        {
            ModelState.AddModelError("Nombres", "Los nombres son obligatorios.");
        }
        else
        {
            if (!ValidadorEntrada.SoloLetras(autor.Nombres))
            {
                ModelState.AddModelError("Nombres", "Los nombres solo pueden contener letras y espacios.");
            }
            else if (ValidadorEntrada.ExcedeLongitud(autor.Nombres, 100))
            {
                ModelState.AddModelError("Nombres", "Los nombres exceden la longitud máxima de 100 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(autor.Apellidos))
        {
            if (!ValidadorEntrada.SoloLetras(autor.Apellidos))
            {
                ModelState.AddModelError("Apellidos", "Los apellidos solo pueden contener letras y espacios.");
            }
            else if (ValidadorEntrada.ExcedeLongitud(autor.Apellidos, 100))
            {
                ModelState.AddModelError("Apellidos", "Los apellidos exceden la longitud máxima de 100 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(autor.Nacionalidad))
        {
            if (!ValidadorEntrada.SoloLetras(autor.Nacionalidad))
            {
                ModelState.AddModelError("Nacionalidad", "La nacionalidad solo puede contener letras y espacios.");
            }
            else if (ValidadorEntrada.ExcedeLongitud(autor.Nacionalidad, 100))
            {
                ModelState.AddModelError("Nacionalidad", "La nacionalidad excede la longitud máxima de 100 caracteres.");
            }
        }

        if (!ValidadorEntrada.FechaNoFutura(autor.FechaNacimiento))
        {
            ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser futura.");
        }

        if (!ModelState.IsValid)
        {
            OnGet();
            return Page();
        }

        var repository = _autorRepositoryFactory.CreateRepository();
        repository.Update(autor);

        return RedirectToPage();
    }
}