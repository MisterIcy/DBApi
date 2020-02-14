using System;
using System.Collections.Generic;
using System.Text;

namespace DBApi
{
    /// <summary>
    /// Common interface for Entity Managers
    /// </summary>
    public interface IEntityManager
    {
        T Persist<T>(T entityObject) where T : class;
        object Persist(Type entityType, object entityObject);
        T Update<T>(T entityObject) where T : class;
        object Update(Type entityType, object entityObject);
        T FindById<T>(object identifier) where T : class;
        object FindById(Type entityType, object identifier);
        T FindOneBy<T>(Dictionary<string, object> parameters) where T : class;
        object FindOneBy(Type entityType, Dictionary<string, object> parameters);
        List<T> FindBy<T>(Dictionary<string, object> parameters);
        List<object> FindBy(Type entityType, Dictionary<string, object> parameters);
        List<T> FindAll<T>() where T: class;
        List<object> FindAll(Type entityType);
        void Delete<T>(T entityObject) where T : class;
        void Delete(Type entityType, object entityObject);

    }
}
