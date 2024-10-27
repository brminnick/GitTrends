using System.Collections;

namespace GitTrends.UnitTests;

public class MockPreferences : IPreferences
{
	static readonly Lazy<Hashtable> _hashtableHolder = new();

	static Hashtable Hashtable => _hashtableHolder.Value;

	public void Clear(string? sharedName) => Hashtable.Clear();

	public bool ContainsKey(string key, string? sharedName) => Hashtable.ContainsKey(key);

	public T Get<T>(string key, T defaultValue, string? sharedName)
	{
		if (Hashtable.ContainsKey(key))
			return (T)(Hashtable[key] ?? throw new InvalidOperationException());

		return defaultValue;
	}
	public void Set<T>(string key, T value, string? sharedName)
	{
		if (Hashtable.ContainsKey(key))
			Hashtable[key] = value;
		else
			Hashtable.Add(key, value);
	}

	public void Remove(string key, string? sharedName = null)
	{
		if (Hashtable.ContainsKey(key))
			Hashtable.Remove(key);
	}
}