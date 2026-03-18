using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class AutorModel : PageModel
{
    public DataTable AutorDataTable { get; set; } = new DataTable();

    private readonly RepositoryFactory<Autor,int> _autorRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Orden { get; set; }

    public AutorModel(RepositoryFactory<Autor,int> autorRepositoryFactory, RouteTokenService routeTokenService)
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
}