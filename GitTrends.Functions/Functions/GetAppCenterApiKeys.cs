using System;
using System.Net;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    public static class GetAppCenterApiKeys
    {
        readonly static string _iOS = Environment.GetEnvironmentVariable("AppCenterApiKey_iOS") ?? string.Empty;
        readonly static string _android = Environment.GetEnvironmentVariable("AppCenterApiKey_Android") ?? string.Empty;

        [Function(nameof(GetAppCenterApiKeys))]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req, FunctionContext context)
        {
            var log = context.GetLogger(nameof(GetAppCenterApiKeys));

            log.LogInformation("Retrieving Client Id");

            if (string.IsNullOrWhiteSpace(_iOS))
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"{nameof(_iOS)} Not Found").ConfigureAwait(false);

                return notFoundResponse;
            }
            else if (string.IsNullOrWhiteSpace(_android))
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"{nameof(_android)} Not Found").ConfigureAwait(false);

                return notFoundResponse;
            }
            else
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new AppCenterApiKeyDTO(_iOS, _android)).ConfigureAwait(false);

                return response;
            }
        }
    }
}
