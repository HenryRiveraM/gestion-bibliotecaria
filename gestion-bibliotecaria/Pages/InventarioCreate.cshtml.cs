using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class InventarioCreateModel : PageModel
{
    private readonly DatabaseService _databaseService;

    [BindProperty]
    public Ejemplar Ejemplar { get; set; } = new Ejemplar { Estado = true, Disponible = true };

    public List<Libro> Libros { get; set; } = new List<Libro>();

    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public InventarioCreateModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task OnGetAsync()
    {
        Libros = await _databaseService.ObtenerLibros();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor completa todos los campos requeridos.";
            Libros = await _databaseService.ObtenerLibros();
            return Page();
        }

        try
        {
            var ejemplarId = await _databaseService.InsertarEjemplar(Ejemplar);
            SuccessMessage = $"Ejemplar agregado exitosamente al inventario con ID: {ejemplarId}";
            
            return RedirectToPage("Inventario");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al agregar el ejemplar: {ex.Message}";
            Libros = await _databaseService.ObtenerLibros();
            return Page();
        }
    }
}
