using System.Text.Json.Serialization;

namespace  GitTrends.Common;

public abstract record GraphQLRequest
{
	protected GraphQLRequest(string query, string variables = "") => (Query, Variables) = (query, variables);

	[JsonPropertyName("query")]
	public string Query { get; }

	[JsonPropertyName("variables")]
	public string Variables { get; }
}