using System;
using System.Collections.Generic;
using System.Text;

namespace DBApi
{
    public class GenericRepository<T> : IRepository<T> where T: class
    {
        private readonly IEntityManager entityManager;
        public GenericRepository(IEntityManager entityManager)
        {
            this.entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
        }

        public bool Delete(T entityObject)
        {
            this.entityManager.Delete(entityObject);
            return true;
        }

        public List<T> FindAll()
        {
            return this.entityManager.FindAll<T>();
        }

        public List<T> FindBy(Dictionary<string, object> parameters)
        {
            return this.entityManager.FindBy<T>(parameters);
        }

        public T FindById(object identifier)
        {
            return entityManager.FindById<T>(identifier);
        }

        public T FindOneBy(Dictionary<string, object> parameters)
        {
            return this.entityManager.FindOneBy<T>(parameters);
        }

        public IEntityManager GetEntityManager()
        {
            return this.entityManager;
        }

        public T Persist(T entityObject)
        {
            return this.Persist(entityObject);
        }

        public T Update(T entityObject)
        {
            return this.Update(entityObject);
        }
    }
}
