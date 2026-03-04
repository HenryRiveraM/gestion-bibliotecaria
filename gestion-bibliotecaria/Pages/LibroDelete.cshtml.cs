using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class LibroDeleteModel : PageModel
{
    private readonly DatabaseService _databaseService;

    public Libro Libro { get; set; } = new Libro();

    public string ErrorMessage { get; set; } = string.Empty;

    public LibroDeleteModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var libro = await _databaseService.ObtenerLibroPorId(id);
        
        if (libro == null)
        {
            return NotFound();
        }

        Libro = libro;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            var success = await _databaseService.EliminarLibro(id);
            
            if (success)
            {
                return RedirectToPage("Libro");
            }
            else
            {
                ErrorMessage = "No se pudo eliminar el libro.";
                var libro = await _databaseService.ObtenerLibroPorId(id);
                if (libro != null)
                {
                    Libro = libro;
                }
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al eliminar el libro: {ex.Message}";
            var libro = await _databaseService.ObtenerLibroPorId(id);
            if (libro != null)
            {
                Libro = libro;
            }
            return Page();
        }
    }
}
