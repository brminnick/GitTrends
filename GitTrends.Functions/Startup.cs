using System;
using System.Net;
using System.Net.Http;
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
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddRefitClient<IGitHubAuthApi>()
              .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubBaseUrl))
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddSingleton<GitHubAuthService>();

            static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
        }
    }
}
