using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class InventarioModel : PageModel
{
    private readonly DatabaseService _databaseService;

    public List<Ejemplar> Ejemplares { get; set; } = new List<Ejemplar>();
    public Dictionary<int, string> LibrosTitulos { get; set; } = new Dictionary<int, string>();

    public InventarioModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task OnGetAsync()
    {
        Ejemplares = await _databaseService.ObtenerEjemplares();
        
        // Obtener los títulos de los libros
        var libros = await _databaseService.ObtenerLibros();
        foreach (var libro in libros)
        {
            LibrosTitulos[libro.LibroId] = libro.Titulo;
        }
    }
}
