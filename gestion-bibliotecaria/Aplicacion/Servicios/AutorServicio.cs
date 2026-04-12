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

    public DataTable Select() 
    {
        var autores = _autorRepositorio.GetAll();
        var dt = new DataTable();
        dt.Columns.Add("AutorId", typeof(int));
        dt.Columns.Add("UsuarioSesionId", typeof(int));
        dt.Columns.Add("Nombres", typeof(string));
        dt.Columns.Add("Apellidos", typeof(string));
        dt.Columns.Add("Nacionalidad", typeof(string));
        dt.Columns.Add("FechaNacimiento", typeof(DateTime));
        dt.Columns.Add("Estado", typeof(bool));
        dt.Columns.Add("FechaRegistro", typeof(DateTime));
        dt.Columns.Add("UltimaActualizacion", typeof(DateTime));

        foreach(var a in autores)
        {
            dt.Rows.Add(
                a.AutorId,
                a.UsuarioSesionId.HasValue ? a.UsuarioSesionId.Value : DBNull.Value,
                a.Nombres,
                a.Apellidos ?? (object)DBNull.Value,
                a.Nacionalidad ?? (object)DBNull.Value,
                a.FechaNacimiento.HasValue ? a.FechaNacimiento.Value : DBNull.Value,
                a.Estado,
                a.FechaRegistro,
                a.UltimaActualizacion.HasValue ? a.UltimaActualizacion.Value : DBNull.Value
            );
        }
        return dt;
    }

    public void Create(Autor autor) => _autorRepositorio.Insert(autor);

    public void Update(Autor autor) => _autorRepositorio.Update(autor);

    public void Delete(Autor autor) => _autorRepositorio.Delete(autor);

    public Autor? GetById(int id) => _autorRepositorio.GetById(id);

    public Dictionary<int, string> ObtenerAutoresActivos() 
    {
        var dict = new Dictionary<int, string>();
        var autores = _autorRepositorio.ObtenerAutoresActivos();
        foreach(var a in autores)
        {
            dict[a.AutorId] = a.Nombres;
        }
        return dict;
    }

    public DataTable ObtenerAutoresActivosTabla() 
    {
        var autores = _autorRepositorio.ObtenerAutoresActivosTabla();
        var dt = new DataTable();
        dt.Columns.Add("AutorId", typeof(int));
        dt.Columns.Add("Nombres", typeof(string));
        dt.Columns.Add("Apellidos", typeof(string));

        foreach(var a in autores)
        {
            dt.Rows.Add(
                a.AutorId,
                a.Nombres,
                a.Apellidos ?? (object)DBNull.Value
            );
        }
        return dt;
    }

    public bool ExisteAutorActivo(int autorId) => _autorRepositorio.ExisteAutorActivo(autorId);

    public Result ValidarAutor(Autor autor)
    {
        // 🔹 Normalizar
        autor.Nombres = ValidadorEntrada.NormalizarAMayusculas(autor.Nombres);
        autor.Apellidos = ValidadorEntrada.NormalizarAMayusculas(autor.Apellidos);
        autor.Nacionalidad = ValidadorEntrada.NormalizarEspacios(autor.Nacionalidad);

        // 🔹 NOMBRES OBLIGATORIOS
        if (ValidadorEntrada.EstaVacio(autor.Nombres))
            return Result.Failure(AutorErrors.NombresObligatorios);

        // 🔹 SOLO LETRAS Y ESPACIOS
        if (!Regex.IsMatch(autor.Nombres, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            return Result.Failure(AutorErrors.NombresFormato);

        // 🔹 LONGITUD
        if (ValidadorEntrada.ExcedeLongitud(autor.Nombres, 100))
            return Result.Failure(AutorErrors.NombresLongitud);

        // 🔹 APELLIDOS (OPCIONAL)
        if (!string.IsNullOrEmpty(autor.Apellidos))
        {
            if (!Regex.IsMatch(autor.Apellidos, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                return Result.Failure(AutorErrors.ApellidosFormato);

            if (ValidadorEntrada.ExcedeLongitud(autor.Apellidos, 100))
                return Result.Failure(AutorErrors.ApellidosLongitud);
        }

        // 🔹 NACIONALIDAD (OPCIONAL)
        if (!string.IsNullOrEmpty(autor.Nacionalidad))
        {
            if (!Regex.IsMatch(autor.Nacionalidad, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                return Result.Failure(AutorErrors.NacionalidadFormato);

            if (ValidadorEntrada.ExcedeLongitud(autor.Nacionalidad, 100))
                return Result.Failure(AutorErrors.NacionalidadLongitud);
        }

        // 🔹 FECHA
        if (autor.FechaNacimiento.HasValue &&
            autor.FechaNacimiento > DateTime.Now)
            return Result.Failure(AutorErrors.FechaFutura);

        return Result.Success();
    }
}