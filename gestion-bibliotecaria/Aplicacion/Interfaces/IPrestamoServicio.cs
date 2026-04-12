using System.Data;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Aplicacion.Interfaces;

public interface IPrestamoServicio
{
    DataTable Select();
    void Create(Prestamo prestamo);
    void Update(Prestamo prestamo);
    void Delete(Prestamo prestamo);
    Prestamo? GetById(int id);
    Result ValidarPrestamo(Prestamo prestamo);
    int CountPrestamosActivos(int lectorId);
}
