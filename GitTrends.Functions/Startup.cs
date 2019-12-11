using System.Net;
using System.Net.Http;
using GitTrends.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace GitTrends.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient<GitHubAuthServiceClient>().ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });
            builder.Services.AddSingleton<GitHubAuthService>();
        }
    }
}
