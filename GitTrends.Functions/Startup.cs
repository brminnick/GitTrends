using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using GitHubApiStatus.Extensions;
using GitTrends.Functions;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Polly;
using Refit;

[assembly: FunctionsStartup(typeof(Startup))]
namespace GitTrends.Functions
{
    public class Startup : FunctionsStartup
    {
        readonly static string _token = Environment.GetEnvironmentVariable("UITestToken_brminnick") ?? string.Empty;
        readonly static string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddRefitClient<IGitHubAuthApi>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubBaseUrl))
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddRefitClient<IGitHubApiV3>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl))
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddRefitClient<IGitHubGraphQLApi>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi))
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddGitHubApiStatusService(new AuthenticationHeaderValue("bearer", _token), new ProductHeaderValue(nameof(GitTrends)))
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddSingleton<NuGetService>();
            builder.Services.AddSingleton<GitHubAuthService>();
            builder.Services.AddSingleton<GitHubApiV3Service>();
            builder.Services.AddSingleton<BlobStorageService>();
            builder.Services.AddSingleton<GitHubGraphQLApiService>();
            builder.Services.AddSingleton<CloudBlobClient>(CloudStorageAccount.Parse(_storageConnectionString).CreateCloudBlobClient());

            static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
            static DecompressionMethods getDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;
        }
    }
}
