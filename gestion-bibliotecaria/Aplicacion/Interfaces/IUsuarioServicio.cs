using System.Collections.Generic;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Aplicacion.Dtos;

namespace gestion_bibliotecaria.Aplicacion.Interfaces;

public interface IUsuarioServicio
{
    IEnumerable<UsuarioDto> Select();
    Result<Usuario> Login(string nombreUsuario, string passwordPlano);
    Task<Result> CrearUsuarioAsync(UsuarioDto usuarioDto, int usuarioSesionId, CancellationToken cancellationToken = default);
    Result DarDeBaja(int usuarioId, int usuarioSesionId);
    string JoinCiComp(string ci, string complemento);
}
