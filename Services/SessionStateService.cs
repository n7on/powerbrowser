using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerBrowser.Services
{
    public class SessionStateService<T> : ISessionStateService<T>
    {
        private readonly SessionState _sessionState;
        private readonly string _key;

        public SessionStateService(SessionState sessionState, string key)
        {
            _sessionState = sessionState;
            _key = key;
        }

        private Dictionary<string, T> GetStore()
        {
            if (!(_sessionState.PSVariable.GetValue(_key) is Dictionary<string, T> store))
            {
                store = new Dictionary<string, T>();
                _sessionState.PSVariable.Set(_key, store);
            }
            return store;
        }

        public void Save(string name, T value)
        {
            var store = GetStore();
            store[name] = value;
            _sessionState.PSVariable.Set(_key, store);
        }

        public Dictionary<string, T> GetAll()
        {
            return GetStore();
        }

        public T Get(string name)
        {
            var store = GetStore();
            return store.TryGetValue(name, out var value) ? value : default;
        }

        public bool Remove(string name)
        {
            var store = GetStore();
            var removed = store.Remove(name);
            _sessionState.PSVariable.Set(_key, store);
            return removed;
        }

        public void Clear()
        {
            var store = GetStore();
            store.Clear();
            _sessionState.PSVariable.Set(_key, store);
        }
    }
}