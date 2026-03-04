using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class InventarioDeleteModel : PageModel
{
    private readonly DatabaseService _databaseService;

    public Ejemplar Ejemplar { get; set; } = new Ejemplar();
    public string TituloLibro { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public InventarioDeleteModel(DatabaseService databaseService)
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
        TituloLibro = await _databaseService.ObtenerTituloLibro(ejemplar.LibroId);
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            var success = await _databaseService.EliminarEjemplar(id);
            
            if (success)
            {
                return RedirectToPage("Inventario");
            }
            else
            {
                ErrorMessage = "No se pudo eliminar el ejemplar del inventario.";
                var ejemplar = await _databaseService.ObtenerEjemplarPorId(id);
                if (ejemplar != null)
                {
                    Ejemplar = ejemplar;
                    TituloLibro = await _databaseService.ObtenerTituloLibro(ejemplar.LibroId);
                }
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al eliminar el ejemplar: {ex.Message}";
            var ejemplar = await _databaseService.ObtenerEjemplarPorId(id);
            if (ejemplar != null)
            {
                Ejemplar = ejemplar;
                TituloLibro = await _databaseService.ObtenerTituloLibro(ejemplar.LibroId);
            }
            return Page();
        }
    }
}
