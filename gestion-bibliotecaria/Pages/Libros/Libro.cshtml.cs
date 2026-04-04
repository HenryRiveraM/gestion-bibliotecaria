using gestion_bibliotecaria.Aplicacion.Servicios;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Domain.Validations;
using gestion_bibliotecaria.Infrastructure.Creators;
using gestion_bibliotecaria.Infrastructure.Persistence;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class LibroModel : PageModel
{
    private readonly RepositoryFactory<Libro, int> _libroRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;
    private readonly LibroService _libroService;

    public DataTable Libros { get; set; } = new DataTable();
    public Dictionary<int, string> AutoresNombres { get; set; } = new();
    public DataTable Autores { get; set; } = new();

    public LibroModel(
        RepositoryFactory<Libro, int> libroRepositoryFactory,
        RouteTokenService routeTokenService,
        LibroService libroService)
    {
        _libroRepositoryFactory = libroRepositoryFactory;
        _routeTokenService = routeTokenService;
        _libroService = libroService;
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
            Autores = libroRepository.ObtenerAutoresActivos();
        }
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var libroId))
        {
            return NotFound();
        }

        var repository = _libroRepositoryFactory.CreateRepository();
        repository.Delete(new Libro { LibroId = libroId });

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        int? AutorId, // Cambiado a int? para evitar el error Binder de ASP.NET
        string? Titulo, // Agregado ? para soportar nulls antes de validar
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
            // For fetch, NotFound is bad. Return JSON error general.
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return new JsonResult(new { success = false, errors = new Dictionary<string, string> { { "", "Petición inválida o token expirado." } } });
            return NotFound();
        }

        // Mapeamos a la entidad exacta. Manejamos el AutorId nulo convirtiéndolo en 0.
        var libro = new Libro
        {
            LibroId = id,
            AutorId = AutorId ?? 0, // Si es null ( Binder vacío), ponle 0
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

        // Usamos EXACTAMENTE la misma lógica de validación de LibroService que para Crear
        var resultado = _libroService.ValidarLibro(libro, null); // null porque en editar no hay autor nuevo

        if (resultado.IsFailure)
        {
            // Split extrae el nombre del campo (ej: "Titulo")
            ModelState.AddModelError(resultado.Error.Code.Split('.')[1], resultado.Error.Message);
        }
        // Lógica extra que tenías en tu código original
        else if (libro.AutorId != 0 && !EsAutorActivo(libro.AutorId))
        {
            ModelState.AddModelError("AutorId", "El autor seleccionado está inactivo o no existe.");
        }

        // --- CAMBIO PARA FETCH: SI HAY ERRORES MANDA JSON ---
        if (!ModelState.IsValid)
        {
            var listaErrores = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
            );
            return new JsonResult(new { success = false, errors = listaErrores });
        }

        // Persistencia (se mantiene igual)
        var repository = _libroRepositoryFactory.CreateRepository();
        repository.Update(libro);

        // --- CAMBIO PARA FETCH: ÉXITO MANDA JSON ---
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
        string? Descripcion)
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
            Estado = true,
            FechaRegistro = DateTime.Now
        };

        var nombreAutorNormalizado = ValidadorEntrada.NormalizarEspacios(NombreAutorNuevo);
        var resultado = _libroService.ValidarLibro(libro, nombreAutorNormalizado);

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
            var repo = _libroRepositoryFactory.CreateRepository();
            if (repo is LibroRepository lr)
            {
                libro.AutorId = lr.InsertarAutorYObtenerID(nombreAutorNormalizado);
            }
        }

        var repository = _libroRepositoryFactory.CreateRepository();
        repository.Insert(libro);

        return new JsonResult(new { success = true });
    }

    private bool EsAutorActivo(int autorId)
    {
        var repository = _libroRepositoryFactory.CreateRepository();
        return repository is LibroRepository libroRepository && libroRepository.ExisteAutorActivo(autorId);
    }
}