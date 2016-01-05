using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndromedaIT
{
    public class MemoryCache
    {
        /* Internal Pointer */
        private static MemoryCache _Instance;

        /* The Instance */
        public static MemoryCache Instance
        {
            get
            {
                /* Sanity */
                if (_Instance == null)
                    _Instance = new MemoryCache();

                /* Return the internal pointer */
                return _Instance;
            }
        }

        /* List of Cache */
        List<MemoryCacheObject> lCache;
        Object _Lock;

        /* Constructor */
        private MemoryCache()
        {
            /* Setup Cache List */
            lCache = new List<MemoryCacheObject>();
            _Lock = new Object();
        }


        /* Add to cache */
        public void Set(String Key, Object Value, DateTime Expiration)
        {
            /* Allocate */
            MemoryCacheObject mObject = new MemoryCacheObject();
            mObject.Key = Key;
            mObject.Value = Value;
            mObject.Expiration = Expiration;

            /* If key exists we override */
            lock (_Lock)
            {
                int eIndex = lCache.FindIndex(mCache => mCache.Key == Key);
                if (eIndex != -1)
                    lCache.RemoveAt(eIndex);

                /* Add */
                lCache.Add(mObject);
            }
        }

        /* Get from cache */
        public Object Get(String Key)
        {
            /* Find Key */
            int eIndex = lCache.FindIndex(mCache => mCache.Key == Key);
            
            /* Sanity */
            if (eIndex == -1)
                return null;

            /* Check expiration */
            lock (_Lock)
            {
                if (lCache[eIndex].Expiration < DateTime.Now)
                {
                    lCache.RemoveAt(eIndex);
                    return null;
                }

                /* Ok ok, cache exists */
                return lCache[eIndex].Value;
            }
        }

        /* Invalidate Key */
        public void InvalidateKey(String Key)
        {
            lock (_Lock)
            {
                /* Find Key */
                int eIndex = lCache.FindIndex(mCache => mCache.Key == Key);

                /* Sanity */
                if (eIndex == -1)
                    return;

                /* Remove */
                lCache.RemoveAt(eIndex);
            }

            /* Cleanup */
            GC.Collect();
        }

        /* Invalidate Keys of type */
        public void InvalidateKeyOfParts(String[] Keys)
        {
            /* Iterate */
            lock (_Lock)
            {
                lCache.RemoveAll(mco => Keys.All(mco.Key.Contains));
            }

            /* Cleanup */
            GC.Collect();
        }

        /* Invalidate Keys of type */
        public void InvalidateKeysOfType(String TypeKey)
        {
            /* Iterate */
            lock (_Lock)
            {
                lCache.RemoveAll(mco => mco.Key.Contains(TypeKey));
            }

            /* Cleanup */
            GC.Collect();
        }
    }


    /* Cache Object */
    public class MemoryCacheObject
    {
        /* Key */
        public String Key;

        /* Value */
        public Object Value;

        /* Expiration */
        public DateTime Expiration;
    }
}
