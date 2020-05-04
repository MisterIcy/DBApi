using System.Collections.Generic;

namespace DBApi
{
    public interface IRepository<T>
    {
        IEntityManager GetEntityManager();
        List<T> FindAll();
        List<T> FindBy(Dictionary<string, object> parameters);
        T FindOneBy(Dictionary<string, object> parameters);
        T FindById(object identifier);
        T Persist(T entityObject);
        T Update(T entityObject);
        bool Delete(T entityObject);
    }
}
