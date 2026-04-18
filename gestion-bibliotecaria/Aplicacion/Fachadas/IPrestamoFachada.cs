using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using System.Collections.Generic;

namespace gestion_bibliotecaria.Aplicacion.Fachadas;

public interface IPrestamoFachada
{
    IEnumerable<KeyValuePair<int, string>> BuscarEjemplaresActivos(string q);
    IEnumerable<KeyValuePair<int, string>> BuscarLectoresPorCi(string q);
    Result CrearPrestamoMultiple(int lectorId, IEnumerable<int> ejemplarIds, DateTime fechaDevolucionEsperada, int? usuarioSesionId = null, string? observacionesSalida = null);
    Result CrearPrestamo(Prestamo prestamo);
    int CountPrestamosActivos(int lectorId);
    Prestamo? ObtenerPrestamoPorId(int id);
    gestion_bibliotecaria.Aplicacion.Dtos.EjemplarDto? ObtenerEjemplarPorId(int id);
    Result CrearPrestamos(IEnumerable<Prestamo> prestamos);
    gestion_bibliotecaria.Domain.Entities.Usuario? ObtenerUsuarioPorCi(string ci);
    string? ObtenerLabelEjemplar(int ejemplarId);
    List<object> ObtenerTodosLosLectores();
}
