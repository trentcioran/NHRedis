using System;
using NHibernate.Cache;

namespace NHibernate.Caches.Redis
{
    public class ScratchCacheItem
    {
        public Cache.PutParameters.CacheVersionedPutParameters PutParameters
        { 
            get; set;
        }
        public object CurrentCacheValue
        {
            get; set;
        }
        public object NewCacheValue
        {
            get;
            set;
        }
        public ScratchCacheItem(Cache.PutParameters.CacheVersionedPutParameters putParameters)
        {
            PutParameters = putParameters;
        }
        public override string ToString()
        {
            return "ScratchCacheItem: " + PutParameters +
                   ", current cache value = " + CurrentCacheValue +
                   ", new cache value = " + NewCacheValue +
                   "}";
        }
    }
}
