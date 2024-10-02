using System.Text.Json.Serialization;
namespace GitTrends.Shared;

public record GraphQLResponse<T>(
	[property: JsonPropertyName("data")] T Data, 
	[property: JsonPropertyName("errors")] GraphQLError[]? Errors);