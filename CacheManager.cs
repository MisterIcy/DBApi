using DBApi.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;

namespace DBApi
{
    public static class CacheManager
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;

        public static string GetCacheKey<T>(object identifier) where T: class
        {
            return GetCacheKey(typeof(T), identifier);
        }
        public static string GetCacheKey(Type entityType, object identifier)
        {
            return $"{entityType.ToString()}:{identifier.ToString()}";
        }

        public static T Get<T>(object identifier) where T: class
        {
            return Get(typeof(T), identifier) as T;
        }
        public static object Get(Type entityType, object identifier)
        {
            string key = GetCacheKey(entityType, identifier);
            if (Contains(entityType, identifier))
                return Cache.Get(key);
            return null;

        }
        public static bool Add<T>(T entityObject) where T: class
        {
            return Add(typeof(T), entityObject);
        }
        public static bool Add(Type entityType, object entityObject)
        {
            ClassMetadata metadata = MetadataCache.Get(entityType);
            if (metadata.NoCache)
                return false;

            try
            {
                return Cache.Add(
                    GetCacheKey(entityType, metadata.GetIdentifierField().GetValue(entityObject)),
                    entityObject,
                    DateTime.Now.AddSeconds(metadata.CacheDuration));
            } catch (TargetException tex)
            {
                return false;
            }
        }
        public static bool Contains<T>(object identifier) where T: class
        {
            return Contains(typeof(T), identifier);
        }
        public static bool Contains(Type entityType, object identifier)
        {
            return Cache.Contains(GetCacheKey(entityType, identifier));
        }
        public static bool Remove<T>(T entityObject) where T: class
        {
            return Remove(typeof(T), entityObject);
        }
        public static bool Remove(Type entityType, object entityObject)
        {
            ClassMetadata metadata = MetadataCache.Get(entityType);
            return RemoveById(entityType, metadata.GetIdentifierField().GetValue(entityObject));
        }
        public static bool RemoveById(Type entityType, object identifier)
        {
            if (Contains(entityType, identifier))
            {
                Cache.Remove(GetCacheKey(entityType, identifier));
                return true;
            }
            return false;
        }
        
    }
}
