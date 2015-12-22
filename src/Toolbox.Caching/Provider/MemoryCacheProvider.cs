using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace Toolbox.Caching
{
    public class MemoryCacheProvider : ICacheProvider, IDisposable
    {
        private readonly static List<Tuple<string, Delegate, PropertyInfo>> _entityRegistration = new List<Tuple<string, Delegate, PropertyInfo>>();

        public MemoryCacheProvider()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        private MemoryCache _memoryCache;

        public void Add<T>(string key, T value, bool overwrite = false)
        {
            T v;

            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"{nameof(key)} cannot be empty.", nameof(key));
            }

            if (!_memoryCache.TryGetValue(key, out v) || overwrite)
            {
                _memoryCache.Set(key, value);
            }
        }

        public T Get<T>(string key)
        {
            T value;
            if (_memoryCache.TryGetValue(key, out value))
            {
                return value;
            }
            return default(T);
        }

        public void Clear<T>(string key)
        {
            T value;
            if (_memoryCache.TryGetValue(key, out value))
            {
                _memoryCache.Remove(key);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
					if ( _memoryCache  != null )
					{
						_memoryCache.Dispose();
						_memoryCache = null;
					}
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
