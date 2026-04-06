using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Validations;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class LibroModel : PageModel
{
    private readonly RouteTokenService _routeTokenService;
    private readonly ILibroServicio _libroServicio;

    public DataTable Libros { get; set; } = new DataTable();
    public Dictionary<int, string> AutoresNombres { get; set; } = new();
    public DataTable Autores { get; set; } = new();

    public LibroModel(
        RouteTokenService routeTokenService,
        ILibroServicio libroServicio)
    {
        _routeTokenService = routeTokenService;
        _libroServicio = libroServicio;
    }

    public void OnGet()
    {
        Libros = _libroServicio.Select();

        if (!Libros.Columns.Contains("LibroToken"))
        {
            Libros.Columns.Add("LibroToken", typeof(string));
        }

        foreach (DataRow row in Libros.Rows)
        {
            var libroId = row.Field<int>("LibroId");
            row["LibroToken"] = _routeTokenService.CrearToken(libroId);
        }

        AutoresNombres = _libroServicio.ObtenerNombresAutores();
        Autores = _libroServicio.ObtenerAutoresActivos();
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var libroId))
        {
            return NotFound();
        }

        _libroServicio.Delete(new Libro { LibroId = libroId });

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        int? AutorId,
        string? Titulo,
        string? ISBN,
        string? Editorial,
        string? Genero,
        string? Edicion,
        int? AñoPublicacion,
        int? NumeroPaginas,
        string? Idioma,
        string? PaisPublicacion,
        string? Descripcion,
        bool Estado)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return new JsonResult(new { success = false, errors = new Dictionary<string, string> { { "", "Petición inválida o token expirado." } } });
            return NotFound();
        }

        var libro = new Libro
        {
            LibroId = id,
            AutorId = AutorId ?? 0,
            Titulo = Titulo,
            ISBN = ISBN,
            Editorial = Editorial,
            Genero = Genero,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            NumeroPaginas = NumeroPaginas,
            Idioma = Idioma,
            PaisPublicacion = PaisPublicacion,
            Descripcion = Descripcion,
            Estado = Estado,
            UltimaActualizacion = DateTime.Now
        };

        var resultado = _libroServicio.ValidarLibro(libro, null);

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(resultado.Error.Code.Split('.')[1], resultado.Error.Message);
        }
        else if (libro.AutorId != 0 && !EsAutorActivo(libro.AutorId))
        {
            ModelState.AddModelError("AutorId", "El autor seleccionado está inactivo o no existe.");
        }

        if (!ModelState.IsValid)
        {
            var listaErrores = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
            );
            return new JsonResult(new { success = false, errors = listaErrores });
        }

        _libroServicio.Update(libro);

        return new JsonResult(new { success = true });
    }

    public IActionResult OnPostCrear(
        string? NombreAutorNuevo,
        string? Titulo,
        string? ISBN,
        string? Editorial,
        string? Genero,
        string? Edicion,
        int? AñoPublicacion,
        int? NumeroPaginas,
        string? Idioma,
        string? PaisPublicacion,
        string? Descripcion,
        bool Estado = true)
    {
        ModelState.Remove("AutorId");

        int? AutorId = null;
        if (int.TryParse(Request.Form["AutorId"], out var parsedId)) AutorId = parsedId;

        var libro = new Libro
        {
            AutorId = AutorId ?? 0,
            Titulo = Titulo,
            ISBN = ISBN,
            Editorial = Editorial,
            Genero = Genero,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            NumeroPaginas = NumeroPaginas,
            Idioma = Idioma,
            PaisPublicacion = PaisPublicacion,
            Descripcion = Descripcion,
            Estado = Estado,
            FechaRegistro = DateTime.Now
        };

        var nombreAutorNormalizado = ValidadorEntrada.NormalizarEspacios(NombreAutorNuevo);
        var resultado = _libroServicio.ValidarLibro(libro, nombreAutorNormalizado);

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(resultado.Error.Code.Split('.')[1], resultado.Error.Message);
        }
        else if (libro.AutorId != 0 && !EsAutorActivo(libro.AutorId))
        {
            ModelState.AddModelError("AutorId", "El autor seleccionado está inactivo o no existe.");
        }

        if (!ModelState.IsValid)
        {
            var listaErrores = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
            );
            return new JsonResult(new { success = false, errors = listaErrores });
        }

        if (libro.AutorId == 0 && !string.IsNullOrWhiteSpace(nombreAutorNormalizado))
        {
            libro.AutorId = _libroServicio.InsertarAutorYObtenerID(nombreAutorNormalizado);
        }

        _libroServicio.Create(libro);

        return new JsonResult(new { success = true });
    }

    private bool EsAutorActivo(int autorId)
    {
        return _libroServicio.ExisteAutorActivo(autorId);
    }
}