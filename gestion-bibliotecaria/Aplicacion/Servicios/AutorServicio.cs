using System.Collections.Generic;
using System.Linq;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Domain.Factories;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public class AutorServicio : IAutorServicio
{
    private readonly IAutorRepositorio _autorRepositorio;

    public AutorServicio(IAutorRepositorio autorRepositorio)
    {
        _autorRepositorio = autorRepositorio;
    }

    public IEnumerable<AutorDto> Select() 
    {
        var autores = _autorRepositorio.GetAll();
        return autores.Select(a => new AutorDto
        {
            AutorId = a.AutorId,
            Nombres = a.Nombres,
            Apellidos = a.Apellidos,
            Nacionalidad = a.Nacionalidad,
            FechaNacimiento = a.FechaNacimiento,
            Estado = a.Estado,
            RouteToken = a.RouteToken
        });
    }

    public Result<AutorDto> Create(AutorDto autorDto)
    {
        var result = AutorFactory.Crear(
            0,
            null, // UsuarioSesionId will be handled if needed, or we just pass null for now
            autorDto.Nombres,
            autorDto.Apellidos,
            autorDto.Nacionalidad,
            autorDto.FechaNacimiento,
            autorDto.Estado
        );

        if (result.IsFailure)
        {
            return Result<AutorDto>.Failure(result.Error);
        }

        var autor = result.Value;
        _autorRepositorio.Insert(autor);
        
        autorDto.AutorId = autor.AutorId;
        return Result<AutorDto>.Success(autorDto);
    }

    public Result<AutorDto> Update(AutorDto autorDto)
    {
        var autorExistente = _autorRepositorio.GetById(autorDto.AutorId);
        if (autorExistente == null)
        {
            return Result<AutorDto>.Failure(AutorErrors.AutorNoEncontrado);
        }

        var result = AutorFactory.Crear(
            autorDto.AutorId,
            autorExistente.UsuarioSesionId,
            autorDto.Nombres,
            autorDto.Apellidos,
            autorDto.Nacionalidad,
            autorDto.FechaNacimiento,
            autorDto.Estado
        );

        if (result.IsFailure)
        {
            return Result<AutorDto>.Failure(result.Error);
        }

        var autor = result.Value;
        // Keep the old token and dates as they are managed by repo or DB usually, but let's copy needed stuff
        autor.RouteToken = autorExistente.RouteToken;
        autor.FechaRegistro = autorExistente.FechaRegistro;

        _autorRepositorio.Update(autor);
        return Result<AutorDto>.Success(autorDto);
    }

    public Result Delete(int autorId)
    {
        var autor = _autorRepositorio.GetById(autorId);
        if (autor == null)
            return Result.Failure(AutorErrors.AutorNoEncontrado);
            
        _autorRepositorio.Delete(autor);
        return Result.Success();
    }

    public AutorDto? GetById(int id)
    {
        var a = _autorRepositorio.GetById(id);
        if (a == null) return null;

        return new AutorDto
        {
            AutorId = a.AutorId,
            Nombres = a.Nombres,
            Apellidos = a.Apellidos,
            Nacionalidad = a.Nacionalidad,
            FechaNacimiento = a.FechaNacimiento,
            Estado = a.Estado,
            RouteToken = a.RouteToken
        };
    }

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

    public bool ExisteAutorActivo(int autorId) => _autorRepositorio.ExisteAutorActivo(autorId);
}
