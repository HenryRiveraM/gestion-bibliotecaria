using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class LibroModel : PageModel
{
    private readonly RepositoryFactory<Libro, int> _libroRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;

    public DataTable Libros { get; set; } = new DataTable();
    public Dictionary<int, string> AutoresNombres { get; set; } = new();

    public LibroModel(
        RepositoryFactory<Libro, int> libroRepositoryFactory,
        RouteTokenService routeTokenService)
    {
        _libroRepositoryFactory = libroRepositoryFactory;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        var repository = _libroRepositoryFactory.CreateRepository();
        Libros = repository.GetAll();

        if (!Libros.Columns.Contains("LibroToken"))
        {
            Libros.Columns.Add("LibroToken", typeof(string));
        }

        foreach (DataRow row in Libros.Rows)
        {
            var libroId = row.Field<int>("LibroId");
            row["LibroToken"] = _routeTokenService.CrearToken(libroId);
        }

        if (repository is LibroRepository libroRepository)
        {
            AutoresNombres = libroRepository.ObtenerNombresAutores();
        }
    }
}