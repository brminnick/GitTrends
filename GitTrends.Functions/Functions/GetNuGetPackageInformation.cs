using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    public class GetGitTrendsCSProjPaths
    {
        readonly static string _iOSCSProjPath = Environment.GetEnvironmentVariable("iOSCSProjPath") ?? string.Empty;
        readonly static string _uiTestCSProjPath = Environment.GetEnvironmentVariable("UITestCSProjPath") ?? string.Empty;
        readonly static string _androidCSProjPath = Environment.GetEnvironmentVariable("AndroidCSProjPath") ?? string.Empty;
        readonly static string _unitTestCSProjPath = Environment.GetEnvironmentVariable("UnitTestCSProjPath") ?? string.Empty;
        readonly static string _gitTrendsCSProjPath = Environment.GetEnvironmentVariable("GitTrendsCSProjPath") ?? string.Empty;
        readonly static string _functionsCSProjPath = Environment.GetEnvironmentVariable("FunctionsCSProjPath") ?? string.Empty;
        readonly static string _mobileCommonCSProjPath = Environment.GetEnvironmentVariable("MobileCommonCSProjPath") ?? string.Empty;

        [FunctionName(nameof(GetGitTrendsCSProjPaths))]
        public static IReadOnlyList<string> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request, ILogger log) => new[]
        {
            _iOSCSProjPath,
            _uiTestCSProjPath,
            _androidCSProjPath,
            _unitTestCSProjPath,
            _gitTrendsCSProjPath,
            _functionsCSProjPath,
            _mobileCommonCSProjPath
        };
    }
}