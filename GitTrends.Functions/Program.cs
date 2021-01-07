using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GitHubApiStatus.Extensions;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker.Configuration;
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

        static Task Main(string[] args)
        {
#if DEBUG
            Debugger.Launch();
#endif
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(args);
                            
                })
                .ConfigureFunctionsWorker((hostBuilderContext, workerApplicationBuilder) =>
                {
                    workerApplicationBuilder.UseFunctionExecutionMiddleware();
                })
                .ConfigureServices(services =>
                {
                    services.AddLogging();

                    services.AddRefitClient<IGitHubAuthApi>()
                        .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubBaseUrl))
                        .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

                    services.AddRefitClient<IGitHubApiV3>()
                        .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl))
                        .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

                    services.AddRefitClient<IGitHubGraphQLApi>()
                        .ConfigureHttpClient(client => client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi))
                        .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

                    services.AddGitHubApiStatusService(new AuthenticationHeaderValue("bearer", _token), new ProductHeaderValue(nameof(GitTrends)))
                        .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
                        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

                    services.AddSingleton<GitHubAuthService>();
                    services.AddSingleton<GitHubApiV3Service>();
                    services.AddSingleton<GitHubGraphQLApiService>();

                    static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
                    static DecompressionMethods getDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;
                })
                .Build();

            return host.RunAsync();
        }
    }
}
