using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class MockSecureStorage : ISecureStorage
    {
        readonly Dictionary<string, string> _dictionary = new();

        public Task<string?> GetAsync(string key)
        {
            try
            {
                return Task.FromResult<string?>(_dictionary[key]);
            }
            catch
            {
                return Task.FromResult<string?>(null);
            }
        }

        public bool Remove(string key)
        {
            try
            {
                _dictionary.Remove(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void RemoveAll() => _dictionary.Clear();

        public Task SetAsync(string key, string value)
        {
            if (_dictionary.ContainsKey(key))
                _dictionary[key] = value;
            else
                _dictionary.Add(key, value);

            return Task.CompletedTask;
        }
    }
}
