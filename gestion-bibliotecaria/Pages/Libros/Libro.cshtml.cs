using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.Pages;

public class LibroModel : PageModel
{
    private readonly LibroRepository _repository;
    private readonly RouteTokenService _routeTokenService;

    public DataTable Libros { get; set; } = new DataTable();
    public Dictionary<int, string> AutoresNombres { get; set; } = new();

    public LibroModel(LibroRepository repository, RouteTokenService routeTokenService)
    {
        _repository = repository;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        Libros = _repository.ObtenerLibros();

        if (!Libros.Columns.Contains("LibroToken"))
        {
            Libros.Columns.Add("LibroToken", typeof(string));
        }

        foreach (DataRow row in Libros.Rows)
        {
            var libroId = row.Field<int>("LibroId");
            row["LibroToken"] = _routeTokenService.CrearToken(libroId);
        }

        AutoresNombres = _repository.ObtenerNombresAutores();
    }
}