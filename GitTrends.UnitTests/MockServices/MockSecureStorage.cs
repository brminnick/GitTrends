using System.Collections;

namespace GitTrends.UnitTests;

class MockSecureStorage : ISecureStorage
{
	static readonly Lazy<Hashtable> _hashtableHolder = new();

	static Hashtable Hashtable => _hashtableHolder.Value;

	public Task<string?> GetAsync(string key) => Task.FromResult((string?)Hashtable[key]);

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