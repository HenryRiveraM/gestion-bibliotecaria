using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace gestion_bibliotecaria.Pages;

public class AutorModel : PageModel
{
    private readonly IAutorServicio _autorServicio;
    private readonly RouteTokenService _routeTokenService;

    public List<AutorDto> Autores { get; set; } = new();

    [BindProperty]
    public AutorDto Autor { get; set; } = new AutorDto();

    public string ModalActivo { get; set; } = string.Empty;

    public AutorModel(
        IAutorServicio autorServicio,
        RouteTokenService routeTokenService)
    {
        _autorServicio = autorServicio;
        _routeTokenService = routeTokenService;
    }


    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return RedirectToPage("/Index");
        }

        CargarAutores();
        return Page();
    }

    private void CargarAutores()
    {
        var autores = _autorServicio.Select().ToList();
        
        foreach (var autor in autores)
        {
            if (string.IsNullOrEmpty(autor.RouteToken))
            {
                autor.RouteToken = _routeTokenService.CrearToken(autor.AutorId);
            }
        }
        
        Autores = autores;
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!EsAdminOBibliotecario())
        {
            return RedirectToPage("/Index");
        }

        if (!_routeTokenService.TryObtenerId(token, out var id))
            return NotFound();

        var result = _autorServicio.Delete(id);
        
        if (result.IsFailure)
        {
            // Opcional: manejar error de eliminación
        }

        return RedirectToPage();
    }

    public IActionResult OnPostCrear()
    {
        if (!EsAdminOBibliotecario())
        {
            return RedirectToPage("/Index");
        }

        ModalActivo = "crear";

        var result = _autorServicio.Create(Autor);

        if (result.IsFailure)
        {
            AgregarError(result.Error, true);
        }

        if (!ModelState.IsValid)
        {
            CargarAutores();
            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        string Nombres,
        string? Apellidos,
        string? Nacionalidad,
        DateTime? FechaNacimiento,
        bool? Estado)
    {
        if (!EsAdminOBibliotecario())
        {
            return RedirectToPage("/Index");
        }

        ModalActivo = "editar";

        if (!_routeTokenService.TryObtenerId(token, out var id))
            return NotFound();

        var autorDto = new AutorDto
        {
            AutorId = id,
            Nombres = Nombres,
            Apellidos = Apellidos,
            Nacionalidad = Nacionalidad,
            FechaNacimiento = FechaNacimiento,
            Estado = Estado ?? false
        };

        var result = _autorServicio.Update(autorDto);

        if (result.IsFailure)
        {
            AgregarError(result.Error);
        }

        if (!ModelState.IsValid)
        {
            ModelState.SetModelValue("token", new ValueProviderResult(token));
            ModelState.SetModelValue("Nombres", new ValueProviderResult(Nombres));
            ModelState.SetModelValue("Apellidos", new ValueProviderResult(Apellidos ?? ""));
            ModelState.SetModelValue("Nacionalidad", new ValueProviderResult(Nacionalidad ?? ""));
            ModelState.SetModelValue("FechaNacimiento", new ValueProviderResult(FechaNacimiento?.ToString("yyyy-MM-dd") ?? ""));
            ModelState.SetModelValue("Estado", new ValueProviderResult((Estado ?? false).ToString()));
            CargarAutores();
            return Page();
        }

        return RedirectToPage();
    }

    private void AgregarError(Error error, bool esCrear = false)
    {
        var key = error.Code.Split('.').LastOrDefault() ?? string.Empty;

        if (esCrear)
        {
            ModelState.AddModelError($"Autor.{key}", error.Message);
        }
        else
        {
            ModelState.AddModelError(key, error.Message);
        }
    }

    private bool EsAdminOBibliotecario()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);

        return string.Equals(rol, Usuario.RolAdmin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, Usuario.RolBibliotecario, StringComparison.OrdinalIgnoreCase);
    }
}
