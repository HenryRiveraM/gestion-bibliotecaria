using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class LibroEditModel : PageModel
{
    private readonly DatabaseService _databaseService;

    [BindProperty]
    public Libro Libro { get; set; } = new Libro();

    public List<Autor> Autores { get; set; } = new List<Autor>();

    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public LibroEditModel(DatabaseService databaseService)
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
        Autores = await _databaseService.ObtenerAutores();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor completa todos los campos requeridos.";
            Autores = await _databaseService.ObtenerAutores();
            return Page();
        }

        try
        {
            var success = await _databaseService.ActualizarLibro(Libro);
            
            if (success)
            {
                return RedirectToPage("Libro");
            }
            else
            {
                ErrorMessage = "No se pudo actualizar el libro.";
                Autores = await _databaseService.ObtenerAutores();
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al actualizar el libro: {ex.Message}";
            Autores = await _databaseService.ObtenerAutores();
            return Page();
        }
    }
}
