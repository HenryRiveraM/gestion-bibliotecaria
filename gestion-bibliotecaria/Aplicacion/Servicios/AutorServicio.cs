using System.Data;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Validations;
using System.Text.RegularExpressions;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public class AutorServicio : IAutorServicio
{
    private readonly IAutorRepositorio _autorRepositorio;

    public AutorServicio(IAutorRepositorio autorRepositorio)
    {
        _autorRepositorio = autorRepositorio;
    }

    public DataTable Select() => _autorRepositorio.GetAll();

    public void Create(Autor autor) => _autorRepositorio.Insert(autor);

    public void Update(Autor autor) => _autorRepositorio.Update(autor);

    public void Delete(Autor autor) => _autorRepositorio.Delete(autor);

    public Autor? GetById(int id) => _autorRepositorio.GetById(id);

    public Dictionary<int, string> ObtenerAutoresActivos() => _autorRepositorio.ObtenerAutoresActivos();

    public DataTable ObtenerAutoresActivosTabla() => _autorRepositorio.ObtenerAutoresActivosTabla();

    public bool ExisteAutorActivo(int autorId) => _autorRepositorio.ExisteAutorActivo(autorId);

    public Result ValidarAutor(Autor autor)
    {
        
        autor.Nombres = ValidadorEntrada.NormalizarEspacios(autor.Nombres);
        autor.Apellidos = ValidadorEntrada.NormalizarEspacios(autor.Apellidos);
        autor.Nacionalidad = ValidadorEntrada.NormalizarEspacios(autor.Nacionalidad);

        
        if (ValidadorEntrada.EstaVacio(autor.Nombres))
            return Result.Failure(AutorErrors.NombresObligatorios);

        
        if (!Regex.IsMatch(autor.Nombres, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            return Result.Failure(AutorErrors.NombresFormato);

        
        if (ValidadorEntrada.ExcedeLongitud(autor.Nombres, 100))
            return Result.Failure(AutorErrors.NombresLongitud);

        
        if (!string.IsNullOrEmpty(autor.Apellidos))
        {
            if (!Regex.IsMatch(autor.Apellidos, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                return Result.Failure(AutorErrors.ApellidosFormato);

            if (ValidadorEntrada.ExcedeLongitud(autor.Apellidos, 100))
                return Result.Failure(AutorErrors.ApellidosLongitud);
        }

        
        if (!string.IsNullOrEmpty(autor.Nacionalidad))
        {
            if (!Regex.IsMatch(autor.Nacionalidad, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                return Result.Failure(AutorErrors.NacionalidadFormato);

            if (ValidadorEntrada.ExcedeLongitud(autor.Nacionalidad, 100))
                return Result.Failure(AutorErrors.NacionalidadLongitud);
        }

        
        if (autor.FechaNacimiento.HasValue &&
            autor.FechaNacimiento > DateTime.Now)
            return Result.Failure(AutorErrors.FechaFutura);

        return Result.Success();
    }
}