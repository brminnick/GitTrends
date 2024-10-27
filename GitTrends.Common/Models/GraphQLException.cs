using System.Net;
using System.Net.Http.Headers;

namespace GitTrends.Common;

public class GraphQLException<T>(
	in T data,
	in GraphQLError[] errors,
	in HttpStatusCode statusCode,
	in HttpResponseHeaders responseHeaders) : GraphQLException(errors, statusCode, responseHeaders)
{
	public T GraphQLData { get; } = data;
}

public class GraphQLException(
	in GraphQLError[] errors,
	in HttpStatusCode statusCode,
	in HttpResponseHeaders responseHeaders) : Exception
{
	public IReadOnlyList<GraphQLError> Errors { get; } = errors;
	public HttpStatusCode StatusCode { get; } = statusCode;
	public HttpResponseHeaders ResponseHeaders { get; } = responseHeaders;

	public bool IsForbidden()
	{
		return Errors.Any(static x => x.AdditionalEntries?.TryGetValue("type", out var jsonElement) is true && jsonElement.GetString()?.Equals("FORBIDDEN", StringComparison.OrdinalIgnoreCase) is true);
	}

	public bool ContainsSamlOrganizationAuthenticationError(out IReadOnlyList<Uri> ssoUris)
	{
		var doesContainError = ResponseHeaders.TryGetValues("x-github-sso", out var values);

		if (doesContainError)
		{
			var ssoUriList = new List<Uri>();

			foreach (var value in values ?? [])
			{
				var semicolonSeparatedValues = value.Split(';');

				foreach (var semicolonSeparatedValue in semicolonSeparatedValues)
				{
					var urlStartIndex = semicolonSeparatedValue.IndexOf(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase);
					var urlString = urlStartIndex < 0 ? string.Empty : semicolonSeparatedValue[urlStartIndex..];

					var isValidUri = Uri.TryCreate(urlString, UriKind.Absolute, out var uri);

					if (isValidUri && uri != null)
						ssoUriList.Add(uri);
				}
			}

			ssoUris = ssoUriList;
		}
		else
		{
			ssoUris = [];
		}

		return doesContainError;
	}
}