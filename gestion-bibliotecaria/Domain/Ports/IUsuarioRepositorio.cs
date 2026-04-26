using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IUsuarioRepositorio : IRepository<Usuario, int>
{
    Usuario? GetByNombreUsuario(string nombreUsuario);
    bool ExisteNombreUsuario(string nombreUsuario);
    bool ExisteEmail(string email);
    bool ExisteCi(string ci);
    Usuario? GetByCi(string ci);
    string JoinCiComp(string ci, string complemento);
}
