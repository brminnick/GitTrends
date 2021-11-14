using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
	class GetLibraries
	{
		readonly BlobStorageService _blobStorageService;

		public GetLibraries(BlobStorageService blobStorageService) => _blobStorageService = blobStorageService;

		[Function(nameof(GetLibraries))]
		public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, FunctionContext context)
		{
			var log = context.GetLogger<GetLibraries>();
			var nuGetLibraries = await _blobStorageService.GetNuGetLibraries().ConfigureAwait(false);

			var response = request.CreateResponse(System.Net.HttpStatusCode.OK);

			var nuGetLibrariesJson = JsonConvert.SerializeObject(nuGetLibraries);

			await response.WriteStringAsync(nuGetLibrariesJson).ConfigureAwait(false);

			return response;
		}
	}
}