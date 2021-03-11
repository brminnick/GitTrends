using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    public static class GetChartStreamingUrl
    {
        readonly static string _chartVideoManifestUrl = Environment.GetEnvironmentVariable("ChartVideoManifestUrl") ?? string.Empty;

        [Function(nameof(GetChartStreamingUrl))]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext functionContext)
        {
            var logger = functionContext.GetLogger(nameof(GetChartStreamingUrl));
            logger.LogInformation("Retrieving Chart Video");

            if (string.IsNullOrWhiteSpace(_chartVideoManifestUrl))
            {
                var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Chart Video Url not found").ConfigureAwait(false);

                return notFoundResponse;
            }

            var okResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(new StreamingManifest(_chartVideoManifestUrl)).ConfigureAwait(false);

            return okResponse;
        }
    }
}
