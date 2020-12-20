using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

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
            : base(CreateErrorMessage(errors, statusCode))
        {
            Errors = errors;
            StatusCode = statusCode;
            ResponseHeaders = responseHeaders;
        }

        public GraphQLError[] Errors { get; }
        public HttpStatusCode StatusCode { get; }
        public HttpResponseHeaders ResponseHeaders { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(ResponseHeaders)}: {ResponseHeaders}");
            stringBuilder.Append(CreateErrorMessage(Errors, StatusCode));

            return stringBuilder.ToString();
        }

        static string CreateErrorMessage(in GraphQLError[] graphQLErrors, in HttpStatusCode statusCode)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(StatusCode)}: {statusCode}");

            foreach (var error in graphQLErrors)
                stringBuilder.AppendLine(error.Message);

            return stringBuilder.ToString();
        }
    }
}
