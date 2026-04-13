using System.Collections.Generic;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Aplicacion.Dtos;

namespace gestion_bibliotecaria.Aplicacion.Interfaces;

public interface IAutorServicio
{
    IEnumerable<AutorDto> Select();
    Result<AutorDto> Create(AutorDto autorDto);
    Result<AutorDto> Update(AutorDto autorDto);
    Result Delete(int autorId);
    AutorDto? GetById(int id);

    Dictionary<int, string> ObtenerAutoresActivos();
    bool ExisteAutorActivo(int autorId);
}
