using System;
using System.Collections;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class MockSecureStorage : ISecureStorage
    {
        readonly static Lazy<Hashtable> _hashtableHolder = new Lazy<Hashtable>();

        static Hashtable Hashtable => _hashtableHolder.Value;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
        public Task<string> GetAsync(string key) => Task.FromResult<string>((string)Hashtable[key]);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        public bool Remove(string key)
        {
            try
            {
                Hashtable.Remove(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void RemoveAll() => Hashtable.Clear();

        public Task SetAsync(string key, string value)
        {
            if (Hashtable.Contains(key))
                Hashtable[key] = value;
            else
                Hashtable.Add(key, value);

            return Task.CompletedTask;
        }
    }
}
