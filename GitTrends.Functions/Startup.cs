using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using GitHubApiStatus.Extensions;
using GitTrends.Functions;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;

[assembly: FunctionsStartup(typeof(Startup))]
namespace GitTrends.Functions
{
    public class Startup : FunctionsStartup
    {
        readonly static string _token = Environment.GetEnvironmentVariable("UITestToken_brminnick") ?? string.Empty;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddRefitClient<IGitHubAuthApi>()
              .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubBaseUrl))
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddRefitClient<IGitHubApiV3>()
              .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl))
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddRefitClient<IGitHubGraphQLApi>()
              .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi))
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddGitHubApiStatusService(new AuthenticationHeaderValue("bearer", _token), new ProductHeaderValue(nameof(GitTrends)));

            builder.Services.AddSingleton<GitHubAuthService>();
            builder.Services.AddSingleton<GitHubApiV3Service>();
            builder.Services.AddSingleton<GitHubGraphQLApiService>();

            static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
            static DecompressionMethods getDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;
        }
    }
}
