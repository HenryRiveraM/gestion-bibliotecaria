using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.FactoryCreators;

public abstract class RepositoryFactory
{
    public abstract ILibraryRepository CreateRepository();

}
