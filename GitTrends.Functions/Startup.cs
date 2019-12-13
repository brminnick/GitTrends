using System;
using System.Net;
using System.Net.Http;
using GitTrends.Functions;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Refit;

[assembly: FunctionsStartup(typeof(Startup))]
namespace GitTrends.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddRefitClient<IGitHubAuthApi>()
              .ConfigureHttpClient(c => c.BaseAddress = new Uri(GitHubConstants.GitHubAuthBaseUrl))
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getAutomaticDecompressionMethods() });

            builder.Services.AddSingleton<GitHubAuthService>();

            static DecompressionMethods getAutomaticDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;
        }
    }
}
