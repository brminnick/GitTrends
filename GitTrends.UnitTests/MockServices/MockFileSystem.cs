using System.Runtime.CompilerServices;

namespace GitTrends.UnitTests;

public class MockFileSystem : IFileSystem
{
	public string AppDataDirectory { get; } = CreateDirectory();

	public string CacheDirectory { get; } = CreateDirectory();

	public Task<Stream> OpenAppPackageFileAsync(string filename) => throw new NotImplementedException();

	public Task<bool> AppPackageFileExistsAsync(string filename) => throw new NotImplementedException();

	static string CreateDirectory([CallerMemberName] string folderName = "") =>
		Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), folderName)).FullName;
}