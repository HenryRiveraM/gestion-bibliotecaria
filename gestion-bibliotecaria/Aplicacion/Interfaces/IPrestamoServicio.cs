using System.Collections.Generic;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Aplicacion.Dtos;

namespace gestion_bibliotecaria.Aplicacion.Interfaces;

public interface IPrestamoServicio
{
    IEnumerable<PrestamoDto> Select();
    Result<PrestamoDto> Create(PrestamoDto prestamoDto);
    Result<PrestamoDto> Update(PrestamoDto prestamoDto);
    Result Delete(PrestamoDto prestamoDto);
    Prestamo? GetById(int id);
    Result ValidarPrestamo(Prestamo prestamo);
    int CountPrestamosActivos(int lectorId);
    int InsertAndReturnId(Prestamo prestamo);
}
