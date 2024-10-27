using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using GitHubApiStatus.Extensions;
using GitTrends.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;

namespace GitTrends.Functions;

class Program
{
	static readonly string _token = Environment.GetEnvironmentVariable("UITestToken_brminnick") ?? string.Empty;
	static readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;

	static Task Main(string[] args)
	{
		var host = new HostBuilder()
			.ConfigureAppConfiguration(configurationBuilder => configurationBuilder.AddCommandLine(args))
			.ConfigureFunctionsWorkerDefaults()
			.ConfigureServices(services =>
			{
				services.AddLogging();

				services.AddRefitClient<IGitHubAuthApi>()
					.ConfigureHttpClient(static client => client.BaseAddress = new Uri(GitHubConstants.GitHubBaseUrl))
					.ConfigurePrimaryHttpMessageHandler(static config => new HttpClientHandler { AutomaticDecompression = GetDecompressionMethods() })
					.AddStandardResilienceHandler(static options => options.Retry = new ServerlessHttpRetryStrategyOptions());

				services.AddRefitClient<IGitHubApiV3>()
					.ConfigureHttpClient(static client => client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl))
					.ConfigurePrimaryHttpMessageHandler(static config => new HttpClientHandler { AutomaticDecompression = GetDecompressionMethods() })
					.AddStandardResilienceHandler(static options => options.Retry = new ServerlessHttpRetryStrategyOptions());

				services.AddRefitClient<IGitHubGraphQLApi>()
					.ConfigureHttpClient(static client => client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi))
					.ConfigurePrimaryHttpMessageHandler(static config => new HttpClientHandler { AutomaticDecompression = GetDecompressionMethods() })
					.AddStandardResilienceHandler(static options => options.Retry = new ServerlessHttpRetryStrategyOptions());

				services.AddGitHubApiStatusService(new AuthenticationHeaderValue("bearer", _token), new ProductHeaderValue(nameof(GitTrends)))
					.ConfigurePrimaryHttpMessageHandler(static config => new HttpClientHandler { AutomaticDecompression = GetDecompressionMethods() })
					.AddStandardResilienceHandler(static options => options.Retry = new ServerlessHttpRetryStrategyOptions());

				services.AddSingleton<NuGetService>();
				services.AddSingleton<GitHubAuthService>();
				services.AddSingleton<GitHubApiV3Service>();
				services.AddSingleton<BlobStorageService>();
				services.AddSingleton<GitHubGraphQLApiService>();
				services.AddSingleton<BlobServiceClient>(new BlobServiceClient(_storageConnectionString));

				static DecompressionMethods GetDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;

			})
			.Build();

		return host.RunAsync();
	}

	sealed class ServerlessHttpRetryStrategyOptions : HttpRetryStrategyOptions
	{
		public ServerlessHttpRetryStrategyOptions()
		{
			BackoffType = DelayBackoffType.Exponential;
			MaxRetryAttempts = 3;
			UseJitter = true;
			Delay = TimeSpan.FromMilliseconds(200);
		}
	}
}