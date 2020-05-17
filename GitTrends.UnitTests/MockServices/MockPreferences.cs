using System;
using System.Collections;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    public class MockPreferences : IPreferences
    {
        readonly static Lazy<Hashtable> _hashtableHolder = new Lazy<Hashtable>();

        Hashtable Hashtable => _hashtableHolder.Value;

        public void Clear() => Hashtable.Clear();

        public void Clear(string sharedName) => throw new NotImplementedException();

        public bool ContainsKey(string key) => Hashtable.Contains(key);

        public bool ContainsKey(string key, string sharedName) => throw new NotImplementedException();

        public string Get(string key, string defaultValue) => Get<string>(key, defaultValue);

        public bool Get(string key, bool defaultValue) => Get<bool>(key, defaultValue);

        public int Get(string key, int defaultValue) => Get<int>(key, defaultValue);

        public double Get(string key, double defaultValue) => Get<double>(key, defaultValue);

        public float Get(string key, float defaultValue) => Get<float>(key, defaultValue);

        public long Get(string key, long defaultValue) => Get<long>(key, defaultValue);

        public DateTime Get(string key, DateTime defaultValue) => Get<DateTime>(key, defaultValue);

        public string Get(string key, string defaultValue, string sharedName) => throw new NotImplementedException();

        public bool Get(string key, bool defaultValue, string sharedName) => throw new NotImplementedException();

        public int Get(string key, int defaultValue, string sharedName) => throw new NotImplementedException();

        public double Get(string key, double defaultValue, string sharedName) => throw new NotImplementedException();

        public float Get(string key, float defaultValue, string sharedName) => throw new NotImplementedException();

        public long Get(string key, long defaultValue, string sharedName) => throw new NotImplementedException();

        public DateTime Get(string key, DateTime defaultValue, string sharedName) => throw new NotImplementedException();

        public void Remove(string key) => Hashtable.Remove(key);

        public void Remove(string key, string sharedName) => throw new NotImplementedException();

        public void Set(string key, string value) => Set<string>(key, value);

        public void Set(string key, bool value) => Set<bool>(key, value);

        public void Set(string key, int value) => Set<int>(key, value);

        public void Set(string key, double value) => Set<double>(key, value);

        public void Set(string key, float value) => Set<float>(key, value);

        public void Set(string key, long value) => Set<long>(key, value);

        public void Set(string key, DateTime value) => Set<DateTime>(key, value);

        public void Set(string key, string value, string sharedName) => throw new NotImplementedException();

        public void Set(string key, bool value, string sharedName) => throw new NotImplementedException();

        public void Set(string key, int value, string sharedName) => throw new NotImplementedException();

        public void Set(string key, double value, string sharedName) => throw new NotImplementedException();

        public void Set(string key, float value, string sharedName) => throw new NotImplementedException();

        public void Set(string key, long value, string sharedName) => throw new NotImplementedException();


        public void Set(string key, DateTime value, string sharedName) => throw new NotImplementedException();

#pragma warning disable CS8603 // Possible null reference return.
        T Get<T>(string key, T defaultValue)
        {
            if (Hashtable.ContainsKey(key))
                return (T)Hashtable[key];

            return defaultValue;
        }
#pragma warning restore CS8603 // Possible null reference return.

        void Set<T>(string key, T value)
        {
            if (Hashtable.ContainsKey(key))
                Hashtable[key] = value;
            else
                Hashtable.Add(key, value);
        }
    }
}
