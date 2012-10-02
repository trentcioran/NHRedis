﻿using System;
using System.Threading;
using ServiceStack.Redis;
using ServiceStack.Redis.Support;


namespace NHibernate.Caches.Redis
{
    /// <summary>
    /// Garbage collection class. Runs its own thread, doing BLPOP on garbage key list, 
    /// then expiring all keys in list
    /// </summary>
    public class RedisGarbageCollector
    {
        private int _gcRefCount = 0;
        private bool _shouldStop = true;
        private Thread _garbageThread;
        private RedisClient _client;
        private readonly string _host;
        private readonly int _port;

        public RedisGarbageCollector(string host, int port)
        {
            this._host = host;
            this._port = port;
        }

        public void CollectGarbage()
        {
            while (!_shouldStop)
            {
                //BLPOP from garbage list
                //run through keys, expiring all keys in set
                var garbageKeys = _client.BlockingPopItemFromList(RedisNamespace.NamespacesGarbageKey, TimeSpan.FromSeconds(1));
                if (garbageKeys == null) continue;
                var key = _client.PopItemFromSet(garbageKeys);
                //note: garbageKeys set will deleted by Redis when it has zero members
                while ( key != null && !_shouldStop)
                {
                    _client.Expire(key, 0);
                    key = _client.PopItemFromSet(garbageKeys);
                }
            }
        }

        // Start ye olde garbage collection thread
        public void Start()
        {
            lock (this)
            {
                _gcRefCount++;
                if (_gcRefCount != 1) return;
                _shouldStop = false;
                _client = new RedisClient(_host, _port);
                _garbageThread = new Thread(CollectGarbage);
                _garbageThread.Start();
            }

        }
        //Stop ye olde thread
        public void Stop()
        {
            lock (this)
            {
                _gcRefCount--;
                if (_gcRefCount != 0 || _garbageThread == null) return;
                //kill thread
                _shouldStop = true;
                // Use the Join method to block the current thread 
                // until the object's thread terminates.
                _garbageThread.Join();
                _garbageThread = null;
                _client.Dispose();
                _client = null;
            }
        }
    }
}
