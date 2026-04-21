using gestion_bibliotecaria.Domain.Common;

namespace gestion_bibliotecaria.Aplicacion.Fachadas;

public interface IAnulacionFachada
{
    Result AnularPrestamo(int prestamoId, int? usuarioSesionId = null, string? motivo = null);
}