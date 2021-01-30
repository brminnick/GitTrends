using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class NuGetService
    {
        readonly SourceRepository _sourceRepository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

        readonly IPreferences _preferences;

        public NuGetService(IPreferences preferences) => _preferences = preferences;

        public IReadOnlyList<(string title, Uri imageUri)> InstalledNugetPackages
        {
            get
            {
                var serializedInstalledNuGetPackages = _preferences.Get(nameof(InstalledNugetPackages), null);

                return serializedInstalledNuGetPackages is null
                    ? Array.Empty<(string title, Uri imageUri)>()
                    : JsonConvert.DeserializeObject<IReadOnlyList<(string title, Uri imageUri)>>(serializedInstalledNuGetPackages);
            }
            private set
            {
                var serializedInstalledNuGetPackages = JsonConvert.SerializeObject(value);
                _preferences.Set(nameof(InstalledNugetPackages), serializedInstalledNuGetPackages);
            }
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            var installedPackagesList = new List<(string title, Uri imageUri)>();
            await foreach (var installedPackageInfo in GetInstalledPackageInfo(cancellationToken))
            {
                installedPackagesList.Add(installedPackageInfo);
            }

            InstalledNugetPackages = installedPackagesList;
        }

        public async IAsyncEnumerable<(string title, Uri imageUri)> GetInstalledPackageInfo([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var installedNugetPackageNames = GetInstalledPackageNames();
            var metadataResource = await _sourceRepository.GetResourceAsync<PackageMetadataResource>().ConfigureAwait(false);

            var getInstalledPackageInfoTCSList = new List<(string packageName, TaskCompletionSource<(string title, Uri imageUri)?> PackageInfoTCS)>();

            foreach (var name in installedNugetPackageNames)
            {
                getInstalledPackageInfoTCSList.Add((name, new TaskCompletionSource<(string name, Uri imageUri)?>()));
            }

            Parallel.ForEach(getInstalledPackageInfoTCSList, async package =>
            {
                IEnumerable<IPackageSearchMetadata> metadatas = Enumerable.Empty<IPackageSearchMetadata>();

                try
                {
                    metadatas = await metadataResource.GetMetadataAsync(package.packageName, true, true, new SourceCacheContext(), NullLogger.Instance, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {

                }

                var uri = metadatas.FirstOrDefault().IconUrl;
                var title = metadatas.FirstOrDefault().Title;

                if (uri is null || title is null)
                    package.PackageInfoTCS.SetResult(null);
                else
                    package.PackageInfoTCS.SetResult((title, uri));
            });

            var remainingTasks = getInstalledPackageInfoTCSList.Select(x => x.PackageInfoTCS.Task).ToList();

            while (remainingTasks.Any())
            {
                var completedTask = await Task.WhenAny(remainingTasks).ConfigureAwait(false);
                remainingTasks.Remove(completedTask);

                var packageInfo = await completedTask.ConfigureAwait(false);
                if (packageInfo.HasValue)
                    yield return packageInfo.Value;
            }
        }

        static IReadOnlyList<string> GetInstalledPackageNames()
        {
            var referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();

            var nuGetPackages = referencedAssemblies.Where(x => !x.Name.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                                                                && !x.Name.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase));
            return nuGetPackages.Select(x => x.Name).ToList();
        }
    }
}
