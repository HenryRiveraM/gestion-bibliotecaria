using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class LibroModel : PageModel
{
    private readonly DatabaseService _databaseService;

    public List<Libro> Libros { get; set; } = new List<Libro>();

    public LibroModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task OnGetAsync()
    {
        Libros = await _databaseService.ObtenerLibros();
    }
}
