using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GitHubApiStatus.Extensions;
using GitTrends.Shared;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Refit;

namespace GitTrends.Functions
{
    class Program
    {
        readonly static string _token = Environment.GetEnvironmentVariable("UITestToken_brminnick") ?? string.Empty;
        readonly static string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;

        static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder => configurationBuilder.AddCommandLine(args))
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddLogging();

                    services.AddRefitClient<IGitHubAuthApi>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
                        .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubBaseUrl))
                        .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

                    services.AddRefitClient<IGitHubApiV3>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
                        .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl))
                        .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

                    services.AddRefitClient<IGitHubGraphQLApi>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
                        .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi))
                        .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

                    services.AddGitHubApiStatusService(new AuthenticationHeaderValue("bearer", _token), new ProductHeaderValue(nameof(GitTrends)))
                        .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

                    services.AddSingleton<NuGetService>();
                    services.AddSingleton<GitHubAuthService>();
                    services.AddSingleton<GitHubApiV3Service>();
                    services.AddSingleton<BlobStorageService>();
                    services.AddSingleton<GitHubGraphQLApiService>();
                    services.AddSingleton<CloudBlobClient>(CloudStorageAccount.Parse(_storageConnectionString).CreateCloudBlobClient());

                    static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
                    static DecompressionMethods getDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;
                })
                .Build();

            return host.RunAsync();
        }
    }
}
