using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryCreators;

namespace gestion_bibliotecaria.Pages;

public class AutorDeleteModel : PageModel
{
    private readonly RepositoryFactory<Autor> _autorRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor();

    [BindProperty]
    public string AutorToken { get; set; } = string.Empty;

    public AutorDeleteModel(RepositoryFactory<Autor> autorRepositoryFactory, RouteTokenService routeTokenService)
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

        var repository = _autorRepositoryFactory.CreateRepository();
        repository.Delete(Autor);

        return RedirectToPage("Autor");
    }
}