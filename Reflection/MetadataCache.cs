using System;
using System.Linq;
using System.Runtime.Caching;

namespace DBApi.Reflection
{
    /// <summary>
    /// Λανθάνουσα μνήμη μεταδεδομένων οντοτήτων.
    /// </summary>
    public static class MetadataCache
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;

        private static readonly CacheItemPolicy CacheItemPolicy = new CacheItemPolicy { Priority = CacheItemPriority.Default };

        /// <summary>
        /// Αδειάζει όλη την λανθάνουσα μνήμη
        /// </summary>
        public static void ClearCache()
        {
            var keys = Cache.Select(k => k.Key).ToList();
            foreach (string key in keys)
            {
                Cache.Remove(key);
            }
        }
        /// <summary>
        /// Προσθέτει τα μεταδεδομένα μιας οντότητας στην λανθάνουσα μνήμη
        /// </summary>
        /// <typeparam name="T">Ο τύπος της οντότητας</typeparam>
        /// <returns></returns>
        public static bool Add<T>() where T: class
        {
            return Add(typeof(T));
        }
        /// <summary>
        /// Προσθέτει τα μεταδεδομένα μιας οντότητας στην λανθάνουσα μνήμη
        /// </summary>
        /// <param name="entityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        /// <returns></returns>
        public static bool Add(Type entityType)
        {
            ClassMetadata meta = new ClassMetadata(entityType);
            return Cache.Add(meta.CacheKey, meta, CacheItemPolicy);
        }        
        /// <summary>
        /// Φέρνει τα μεταδεδομένα μιας οντότητας από την λανθάνουσα μνήμη
        /// </summary>
        /// <typeparam name="T">Τύπος οντότητας</typeparam>
        /// <returns>Αντικείμενο μεταδεδομένων οντότητας</returns>
        public static ClassMetadata Get<T>() where T: class
        {
            return Get(typeof(T));
        }
        /// <summary>
        /// Φέρνει τα μεταδεδομένα μιας οντότητας από την λανθάνουσα μνήμη
        /// </summary>
        /// <param name="entityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        /// <returns>Αντικείμενο μεταδεδομένων οντότητας</returns>
        public static ClassMetadata Get(Type entityType)
        {
            if (Contains(entityType))
                return Cache.Get(ClassMetadata.GetCacheKey(entityType)) as ClassMetadata;
            else
            {
                Add(entityType);
                return Get(entityType);
                    
            }            
        }
        /// <summary>
        /// Διαγράφει μεταδεδομένα από την λανθάνουσα μνήμη
        /// </summary>
        /// <typeparam name="T">Τύπος οντότητας</typeparam>
        public static void Remove<T>() where T: class
        {
            Remove(typeof(T));
        }
        /// <summary>
        /// Διαγράφει μεταδεδομένα από την λανθάνουσα μνήμη
        /// </summary>
        /// <param name="entityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        public static void Remove(Type entityType)
        {
            if (Contains(entityType))
                Cache.Remove(ClassMetadata.GetCacheKey(entityType));
        }
        /// <summary>
        /// Ελέγχει εάν η λανθάνουσα μνήμη περιέχει τα μεταδεδομένα μιας συγκεκριμένης οντότητας
        /// </summary>
        /// <typeparam name="T">Τύπος οντότητας</typeparam>
        /// <returns>True, αν τα μεταδεδομένα υπάρχουν στην λανθάνουσα μνήμη, αλλιώς false</returns>
        public static bool Contains<T>() where T: class
        {
            return Contains(typeof(T));
        }
        /// <summary>
        /// Ελέγχει εάν η λανθάνουσα μνήμη περιέχει τα μεταδεδομένα μιας συγκεκριμένης οντότητας
        /// </summary>
        /// <param name="entityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        /// <returns>True, αν τα μεταδεδομένα υπάρχουν στην λανθάνουσα μνήμη, αλλιώς false</returns>
        public static bool Contains(Type entityType)
        {
            return Cache.Contains(ClassMetadata.GetCacheKey(entityType));
        }
    }
}
