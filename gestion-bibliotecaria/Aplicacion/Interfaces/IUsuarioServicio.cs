using System.Data;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Aplicacion.Interfaces;

public interface IUsuarioServicio
{
    DataTable Select();
    Result<Usuario> Login(string nombreUsuario, string passwordPlano);
    Task<Result> CrearUsuarioAsync(Usuario usuario, int usuarioSesionId, CancellationToken cancellationToken = default);
    Result DarDeBaja(int usuarioId, int usuarioSesionId);
    string JoinCiComp(string ci, string complemento);
}
