using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class NuGetService
    {
        static readonly HttpClient _client = new();
        readonly SourceRepository _sourceRepository = NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly AzureFunctionsApiService _azureFunctionsApiService;

        public NuGetService(IPreferences preferences,
                            IAnalyticsService analyticsService,
                            GitHubApiV3Service gitHubApiV3Service,
                            AzureFunctionsApiService azureFunctionsApiService)
        {
            _preferences = preferences;
            _analyticsService = analyticsService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _azureFunctionsApiService = azureFunctionsApiService;
        }

        public ReadOnlyDictionary<string, (Uri IconUri, Uri NuGetUri)> InstalledNugetPackages
        {
            get
            {
                var serializedInstalledNuGetPackages = _preferences.Get(nameof(InstalledNugetPackages), null);

                return serializedInstalledNuGetPackages is null
                    ? new ReadOnlyDictionary<string, (Uri, Uri)>(new Dictionary<string, (Uri, Uri)>())
                    : JsonConvert.DeserializeObject<ReadOnlyDictionary<string, (Uri, Uri)>>(serializedInstalledNuGetPackages);
            }
            private set
            {
                var serializedInstalledNuGetPackages = JsonConvert.SerializeObject(value);
                _preferences.Set(nameof(InstalledNugetPackages), serializedInstalledNuGetPackages);
            }
        }

        public async ValueTask Initialize(CancellationToken cancellationToken)
        {
            if (InstalledNugetPackages.Any())
                initialize().SafeFireAndForget(ex => _analyticsService.Report(ex));
            else
                await initialize().ConfigureAwait(false);

            async Task initialize()
            {
                var installedPackagesDictionary = new Dictionary<string, (Uri iconUri, Uri nugetUri)>();

                await foreach (var packageInfo in GetPackageInfo(cancellationToken).ConfigureAwait(false))
                {
                    if (!installedPackagesDictionary.ContainsKey(packageInfo.Title))
                        installedPackagesDictionary.Add(packageInfo.Title, (packageInfo.ImageUri, packageInfo.NugetUri));
                }

                InstalledNugetPackages = new ReadOnlyDictionary<string, (Uri, Uri)>(installedPackagesDictionary);
            }
        }

        public async IAsyncEnumerable<(string Title, Uri ImageUri, Uri NugetUri)> GetPackageInfo([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var packageNames = new List<string>();

            await foreach (var csprojFile in GetCsprojFiles(cancellationToken).ConfigureAwait(false))
            {
                packageNames.AddRange(GetNuGetPackageNames(csprojFile));
            }

            var metadataResource = await _sourceRepository.GetResourceAsync<PackageMetadataResource>().ConfigureAwait(false);

            var getInstalledPackageInfoTCSList = new List<(string PackageName, TaskCompletionSource<(string title, Uri iconUri, Uri nugetUri)?> PackageInfoTCS)>();

            foreach (var name in packageNames)
            {
                getInstalledPackageInfoTCSList.Add((name, new TaskCompletionSource<(string name, Uri iconUri, Uri nugetUri)?>()));
            }

            Parallel.ForEach(getInstalledPackageInfoTCSList, async package =>
            {
                const string defaultNuGetIcon = "https://www.nuget.org/Content/gallery/img/default-package-icon.svg";
                IEnumerable<IPackageSearchMetadata> metadatas = Enumerable.Empty<IPackageSearchMetadata>();

                try
                {
                    metadatas = await metadataResource.GetMetadataAsync(package.PackageName, true, true, new SourceCacheContext(), NullLogger.Instance, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {

                }

                var iconUri = metadatas.FirstOrDefault().IconUrl;
                var nugetUri = metadatas.FirstOrDefault().PackageDetailsUrl;

                if (nugetUri is null)
                    package.PackageInfoTCS.SetResult(null);
                else
                    package.PackageInfoTCS.SetResult((package.PackageName, iconUri ?? new Uri(defaultNuGetIcon), nugetUri));
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

        IReadOnlyList<string> GetNuGetPackageNames(string csProjSourceCode)
        {
            var doc = XDocument.Parse(csProjSourceCode);
            var nugetPackageNames = doc.XPathSelectElements("//PackageReference").Select(pr => pr.Attribute("Include").Value);

            return nugetPackageNames.ToList();
        }

        async IAsyncEnumerable<string> GetCsprojFiles([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var csprojFilePaths = await _azureFunctionsApiService.GetGitTrendsCSProjPaths(cancellationToken).ConfigureAwait(false);

            var getCSProjFileTaskList = new List<(string csprojFilePath, TaskCompletionSource<string> csprojSourceCodeTCS)>();
            foreach (var csprojFilePath in csprojFilePaths)
            {
                getCSProjFileTaskList.Add((csprojFilePath, new TaskCompletionSource<string>()));
            }

            Parallel.ForEach(getCSProjFileTaskList, async getCSProjFileTask =>
            {
                var repositoryFile = await _gitHubApiV3Service.GetGitTrendsFile(getCSProjFileTask.csprojFilePath, cancellationToken).ConfigureAwait(false);
                var file = await _client.GetStringAsync(repositoryFile.DownloadUrl).ConfigureAwait(false);

                getCSProjFileTask.csprojSourceCodeTCS.SetResult(file);
            });

            var remainingTasks = getCSProjFileTaskList.Select(x => x.csprojSourceCodeTCS.Task).ToList();

            while (remainingTasks.Any())
            {
                var completedTask = await Task.WhenAny(remainingTasks).ConfigureAwait(false);
                remainingTasks.Remove(completedTask);

                var csprojFile = await completedTask.ConfigureAwait(false);
                yield return csprojFile;
            }
        }
    }
}
