using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class InventarioEditModel : PageModel
{
    private readonly DatabaseService _databaseService;

    [BindProperty]
    public Ejemplar Ejemplar { get; set; } = new Ejemplar();

    public List<Libro> Libros { get; set; } = new List<Libro>();

    public string ErrorMessage { get; set; } = string.Empty;

    public InventarioEditModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ejemplar = await _databaseService.ObtenerEjemplarPorId(id);
        
        if (ejemplar == null)
        {
            return NotFound();
        }

        Ejemplar = ejemplar;
        Libros = await _databaseService.ObtenerLibros();
        
        return Page();
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
            var success = await _databaseService.ActualizarEjemplar(Ejemplar);
            
            if (success)
            {
                return RedirectToPage("Inventario");
            }
            else
            {
                ErrorMessage = "No se pudo actualizar el ejemplar.";
                Libros = await _databaseService.ObtenerLibros();
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al actualizar el ejemplar: {ex.Message}";
            Libros = await _databaseService.ObtenerLibros();
            return Page();
        }
    }
}
