using gestion_bibliotecaria.Domain.Entities;
using System.Collections.Generic;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IUsuarioRepositorio
{
    IEnumerable<Usuario> GetAll();
    void Insert(Usuario usuario);
    void Update(Usuario usuario);
    void Delete(Usuario usuario);
    Usuario? GetById(int id);
    Usuario? GetByNombreUsuario(string nombreUsuario);
    bool ExisteNombreUsuario(string nombreUsuario);
    bool ExisteEmail(string email);
    Usuario? GetByCi(string ci);
    string JoinCiComp(string ci, string complemento);
}
