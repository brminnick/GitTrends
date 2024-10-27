using System.Text;
using System.Text.Json.Serialization;

namespace  GitTrends.Common;

public record User(
	[property: JsonPropertyName("repositories")] RepositoryConnection RepositoryConnection,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("company")] string Company,
	[property: JsonPropertyName("createdAt")] DateTimeOffset AccountCreationDate,
	[property: JsonPropertyName("login")] string Alias,
	[property: JsonPropertyName("avatarUrl")] Uri AvatarUri)
{

	public override string ToString()
	{
		var stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"{nameof(Name)}: {Name}");
		stringBuilder.AppendLine($"{nameof(Company)}: {Company}");
		stringBuilder.AppendLine($"{nameof(AccountCreationDate)}: {AccountCreationDate}");

		return stringBuilder.ToString();
	}
}