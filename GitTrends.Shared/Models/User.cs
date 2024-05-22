using System.Text;
using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record User
{
	public User(RepositoryConnection repositories, string name, string company, DateTimeOffset createdAt, string login, Uri avatarUrl)
	{
		RepositoryConnection = repositories;
		Name = name;
		Company = company;
		AccountCreationDate = createdAt;
		Alias = login;
		AvatarUri = avatarUrl;
	}

	[JsonPropertyName("repositories")]
	public RepositoryConnection RepositoryConnection { get; }

	[JsonPropertyName("name")]
	public string Name { get; }

	[JsonPropertyName("company")]
	public string Company { get; }

	[JsonPropertyName("createdAt")]
	public DateTimeOffset AccountCreationDate { get; }

	[JsonPropertyName("login")]
	public string Alias { get; }

	[JsonPropertyName("avatarUrl")]
	public Uri AvatarUri { get; }

	public override string ToString()
	{
		var stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"{nameof(Name)}: {Name}");
		stringBuilder.AppendLine($"{nameof(Company)}: {Company}");
		stringBuilder.AppendLine($"{nameof(AccountCreationDate)}: {AccountCreationDate}");

		return stringBuilder.ToString();
	}
}