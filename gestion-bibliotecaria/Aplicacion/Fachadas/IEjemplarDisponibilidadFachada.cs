using gestion_bibliotecaria.Domain.Common;

namespace gestion_bibliotecaria.Aplicacion.Fachadas;

public interface IEjemplarDisponibilidadFachada
{
    Result CambiarDisponibilidad(int ejemplarId, bool disponible, int? usuarioSesionId = null);
}