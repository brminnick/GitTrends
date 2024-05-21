using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace GitTrends.Shared
{
	public class GraphQLException<T> : GraphQLException
	{
		public GraphQLException(in T data,
								in GraphQLError[] errors,
								in HttpStatusCode statusCode,
								in HttpResponseHeaders responseHeaders) : base(errors, statusCode, responseHeaders)
		{
			GraphQLData = data;
		}

		public T GraphQLData { get; }
	}

	public class GraphQLException : Exception
	{
		public GraphQLException(in GraphQLError[] errors,
								in HttpStatusCode statusCode,
								in HttpResponseHeaders responseHeaders)
		{
			Errors = errors;
			StatusCode = statusCode;
			ResponseHeaders = responseHeaders;
		}

		public IReadOnlyList<GraphQLError> Errors { get; }
		public HttpStatusCode StatusCode { get; }
		public HttpResponseHeaders ResponseHeaders { get; }
	}

	public static class GraphQLExceptionExtensions
	{
		public static bool ContainsSamlOrganizationAthenticationError<T>(this GraphQLException<T> graphQLException, out IReadOnlyList<Uri> ssoUris)
		{
			var doesContainError = graphQLException.ResponseHeaders.TryGetValues("x-github-sso", out var values);

			if (doesContainError)
			{
				var ssoUriList = new List<Uri>();

				foreach (var value in values ?? [])
				{
					var semicolonSeparatedValues = value.Split(';');

					foreach (var semicolonSeparatedValue in semicolonSeparatedValues)
					{
						var urlStartIndex = semicolonSeparatedValue.IndexOf("http", StringComparison.OrdinalIgnoreCase);
						var urlString = urlStartIndex < 0 ? string.Empty : semicolonSeparatedValue.Substring(urlStartIndex);

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
}