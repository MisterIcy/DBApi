using System;
using System.Collections.Generic;
using System.Data;
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
        List<T> FindBy<T>(Dictionary<string, object> parameters) where T: class;
        List<object> FindBy(Type entityType, Dictionary<string, object> parameters);
        List<T> FindAll<T>(int currentRetries = 0) where T: class;
        List<object> FindAll(Type entityType, int currentRetries = 0);
        void Delete<T>(T entityObject) where T : class;
        void Delete(Type entityType, object entityObject);

        DataTable GetResult(string query, Dictionary<string, object> parameters = null, int currentRetries = 0);
        DataRow GetSingleResult(string query, Dictionary<string, object> parameters = null, int currentRetries = 0);
        object GetSingleScalarResult(string query, Dictionary<string, object> parameters = null, int currentRetries = 0);

    }
}
