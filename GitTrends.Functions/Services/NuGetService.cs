using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace GitTrends.Functions
{
	class NuGetService
	{
		readonly SourceRepository _sourceRepository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

		readonly static IReadOnlyList<string> _csProjFilePaths = new[]
		{
			Environment.GetEnvironmentVariable("iOSCSProjPath") ?? string.Empty,
			Environment.GetEnvironmentVariable("UITestCSProjPath") ?? string.Empty,
			Environment.GetEnvironmentVariable("AndroidCSProjPath") ?? string.Empty,
			Environment.GetEnvironmentVariable("UnitTestCSProjPath") ?? string.Empty,
			Environment.GetEnvironmentVariable("GitTrendsCSProjPath") ?? string.Empty,
			Environment.GetEnvironmentVariable("FunctionsCSProjPath") ?? string.Empty,
			Environment.GetEnvironmentVariable("MobileCommonCSProjPath") ?? string.Empty
		};

		readonly ILogger _logger;
		readonly HttpClient _client;
		readonly GitHubApiV3Service _gitHubApiV3Service;

		public NuGetService(GitHubApiV3Service gitHubApiV3Service, HttpClient httpClient, ILogger<NuGetService> logger)
		{
			_logger = logger;
			_client = httpClient;
			_gitHubApiV3Service = gitHubApiV3Service;
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
				const string defaultNuGetIcon = "https://www.nuget.org/Content/gallery/img/logo-og-600x600.png";
				IEnumerable<IPackageSearchMetadata> metadatas = Enumerable.Empty<IPackageSearchMetadata>();

				try
				{
					metadatas = await metadataResource.GetMetadataAsync(package.PackageName, true, true, new SourceCacheContext(), NuGet.Common.NullLogger.Instance, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					_logger.LogError(e, e.Message);
				}

				if (metadatas?.Any() is not true)
				{
					package.PackageInfoTCS.SetResult(null);
				}
				else
				{
					var metadataIconUri = metadatas.Last().IconUrl;

					var isIconUrlValid = await isUriValid(metadataIconUri).ConfigureAwait(false);
					var iconUri = isIconUrlValid switch
					{
						true => metadataIconUri,
						false => new Uri(defaultNuGetIcon)
					};

					package.PackageInfoTCS.SetResult((package.PackageName, iconUri, metadatas.Last().PackageDetailsUrl));
				}
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

			async Task<bool> isUriValid(Uri? uri)
			{
				try
				{
					var response = await _client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
					return response.IsSuccessStatusCode;
				}
				catch
				{
					return false;
				}
			}
		}

		static IReadOnlyList<string> GetNuGetPackageNames(string csProjSourceCode)
		{
			var doc = XDocument.Parse(csProjSourceCode);
			var nugetPackageNames = doc.XPathSelectElements("//PackageReference").Select(pr => pr.Attribute("Include")?.Value);

			return nugetPackageNames?.OfType<string>().ToArray() ?? Array.Empty<string>();
		}

		async IAsyncEnumerable<string> GetCsprojFiles([EnumeratorCancellation] CancellationToken cancellationToken)
		{
			var getCSProjFileTaskList = new List<(string csprojFilePath, TaskCompletionSource<string> csprojSourceCodeTCS)>();
			foreach (var csprojFilePath in _csProjFilePaths)
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