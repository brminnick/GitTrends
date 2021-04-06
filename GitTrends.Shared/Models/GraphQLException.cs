using System;
using System.Collections.Generic;
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
}
