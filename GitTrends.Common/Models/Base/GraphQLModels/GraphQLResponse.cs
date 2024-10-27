using System.Text.Json.Serialization;
namespace GitTrends.Common;

public record GraphQLResponse<T>(
	[property: JsonPropertyName("data")] T Data,
	[property: JsonPropertyName("errors")] GraphQLError[]? Errors);