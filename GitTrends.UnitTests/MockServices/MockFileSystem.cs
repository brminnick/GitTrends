using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    public class MockFileSystem : IFileSystem
    {
        public string AppDataDirectory { get; } = CreateDirectory();

        public string CacheDirectory { get; } = CreateDirectory();

        public Task<Stream> OpenAppPackageFileAsync(string filename) => throw new System.NotImplementedException();

        static string CreateDirectory([CallerMemberName] string folderName = "") =>
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), folderName)).FullName;
    }
}
