using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace GitTrends.Functions;

class NuGetService(GitHubApiV3Service gitHubApiV3Service, HttpClient httpClient, ILogger<NuGetService> logger)
{
	readonly SourceRepository _sourceRepository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

	static readonly IReadOnlyList<string> _csProjFilePaths =
	[
		Environment.GetEnvironmentVariable("iOSCSProjPath") ?? string.Empty,
		Environment.GetEnvironmentVariable("UITestCSProjPath") ?? string.Empty,
		Environment.GetEnvironmentVariable("AndroidCSProjPath") ?? string.Empty,
		Environment.GetEnvironmentVariable("UnitTestCSProjPath") ?? string.Empty,
		Environment.GetEnvironmentVariable("GitTrendsCSProjPath") ?? string.Empty,
		Environment.GetEnvironmentVariable("FunctionsCSProjPath") ?? string.Empty,
		Environment.GetEnvironmentVariable("MobileCommonCSProjPath") ?? string.Empty
	];

	readonly ILogger _logger = logger;
	readonly HttpClient _client = httpClient;
	readonly GitHubApiV3Service _gitHubApiV3Service = gitHubApiV3Service;

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
			IEnumerable<IPackageSearchMetadata> metadatas = [];

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


		List<Task<(string Title, Uri IconUri, Uri NuGetUri)?>> remainingTasks = [.. getInstalledPackageInfoTCSList.Select(static x => x.PackageInfoTCS.Task)];

		while (remainingTasks.Count is not 0)
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

		return [.. nugetPackageNames.OfType<string>()];
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
			var file = await _client.GetStringAsync(repositoryFile.DownloadUrl, cancellationToken).ConfigureAwait(false);

			getCSProjFileTask.csprojSourceCodeTCS.SetResult(file);
		});

		List<Task<string>> remainingTasks = [.. getCSProjFileTaskList.Select(static x => x.csprojSourceCodeTCS.Task)];

		while (remainingTasks.Count is not 0)
		{
			var completedTask = await Task.WhenAny(remainingTasks).ConfigureAwait(false);
			remainingTasks.Remove(completedTask);

			var csprojFile = await completedTask.ConfigureAwait(false);
			yield return csprojFile;
		}
	}
}