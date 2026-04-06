using System.Data;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IUsuarioRepositorio
{
    DataTable GetAll();
    void Insert(Usuario usuario);
    void Update(Usuario usuario);
    void Delete(Usuario usuario);
    Usuario? GetById(int id);
    bool ExisteNombreUsuario(string nombreUsuario);
}
