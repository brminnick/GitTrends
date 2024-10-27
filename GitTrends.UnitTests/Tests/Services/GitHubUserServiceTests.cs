using System.Web;
using GitTrends.Common;

namespace GitTrends.UnitTests;

class GitHubUserServiceTests : BaseTest
{
	const string _tokenType = "Bearer";

	readonly string _scope = GitHubConstants.OAuthScope;
	readonly string _token = Guid.NewGuid().ToString();

	[Test]
	public async Task AliasTest()
	{
		//Arrange
		string alias_Initial, aliasChangedResult, alias_Final;

		bool didAliasChangedFire = false;
		var aliasChangedTCS = new TaskCompletionSource<string>();

		GitHubUserService.AliasChanged += HandleAliasChanged;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		alias_Initial = gitHubUserService.Alias;

		//Act
		gitHubUserService.Alias = GitHubConstants.GitTrendsRepoOwner;
		aliasChangedResult = await aliasChangedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
		alias_Final = gitHubUserService.Alias;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didAliasChangedFire, Is.True);
			Assert.That(alias_Initial, Is.EqualTo(string.Empty));
			Assert.That(alias_Final, Is.EqualTo(GitHubConstants.GitTrendsRepoOwner));
			Assert.That(aliasChangedResult, Is.EqualTo(alias_Final));
		});

		void HandleAliasChanged(object? sender, string e)
		{
			GitHubUserService.AliasChanged -= HandleAliasChanged;

			didAliasChangedFire = true;
			aliasChangedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task NameTest()
	{
		//Arrange
		const string expectedName = "Brandon Minnick";

		string name_Initial, nameChangedResult, name_Final;

		bool didNameChangedFire = false;
		var nameChangedTCS = new TaskCompletionSource<string>();

		GitHubUserService.NameChanged += HandleNameChanged;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		name_Initial = gitHubUserService.Name;

		//Act
		gitHubUserService.Name = expectedName;
		nameChangedResult = await nameChangedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
		name_Final = gitHubUserService.Name;

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(didNameChangedFire, Is.True);
			Assert.That(name_Initial, Is.EqualTo(string.Empty));
			Assert.That(name_Final, Is.EqualTo(expectedName));
			Assert.That(nameChangedResult, Is.EqualTo(name_Final));
		});

		void HandleNameChanged(object? sender, string e)
		{
			GitHubUserService.NameChanged -= HandleNameChanged;

			didNameChangedFire = true;
			nameChangedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task AvatarUrlTest()
	{
		//Arrange
		string avatarUrl_Initial, avatarUrlChangedResult, avatarUrl_Final;

		bool didAvatarUrlChangedFire = false;
		var avatarUrlChangedTCS = new TaskCompletionSource<string>();

		GitHubUserService.AvatarUrlChanged += HandleAvatarUrlChanged;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		avatarUrl_Initial = gitHubUserService.AvatarUrl;

		//Act
		gitHubUserService.AvatarUrl = AuthenticatedGitHubUserAvatarUrl;
		avatarUrlChangedResult = await avatarUrlChangedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
		avatarUrl_Final = gitHubUserService.AvatarUrl;

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(didAvatarUrlChangedFire);
			Assert.That(avatarUrl_Initial, Is.EqualTo(string.Empty));
			Assert.That(avatarUrl_Final, Is.EqualTo(AuthenticatedGitHubUserAvatarUrl));
			Assert.That(avatarUrlChangedResult, Is.EqualTo(avatarUrl_Final));
		});

		void HandleAvatarUrlChanged(object? sender, string e)
		{
			GitHubUserService.AvatarUrlChanged -= HandleAvatarUrlChanged;

			didAvatarUrlChangedFire = true;
			avatarUrlChangedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task SaveGitHubTokenTest_ValidToken()
	{
		//Arrange
		var gitHubToken = new GitHubToken(_token, _scope, _tokenType);
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		await gitHubUserService.SaveGitHubToken(gitHubToken).ConfigureAwait(false);
		var retrievedToken = await gitHubUserService.GetGitHubToken().ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(retrievedToken.AccessToken, Is.EqualTo(_token));
			Assert.That(retrievedToken.Scope, Is.EqualTo(_scope));
			Assert.That(retrievedToken.TokenType, Is.EqualTo(_tokenType));

			Assert.That(retrievedToken.AccessToken, Is.EqualTo(gitHubToken.AccessToken));
			Assert.That(retrievedToken.Scope, Is.EqualTo(gitHubToken.Scope));
			Assert.That(retrievedToken.TokenType, Is.EqualTo(gitHubToken.TokenType));
		});
	}

	[Test]
	public async Task SaveGitHubTokenTest_InvalidScopes()
	{
		//Arrange
		var scopes_MissingOrg = HttpUtility.UrlEncode("public_repo read:user");

		var gitHubToken = new GitHubToken(_token, scopes_MissingOrg, _tokenType);
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		await gitHubUserService.SaveGitHubToken(gitHubToken).ConfigureAwait(false);
		var retrievedToken = await gitHubUserService.GetGitHubToken().ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(retrievedToken.AccessToken, Is.EqualTo(GitHubToken.Empty.AccessToken));
			Assert.That(retrievedToken.Scope, Is.EqualTo(GitHubToken.Empty.Scope));
			Assert.That(retrievedToken.TokenType, Is.EqualTo(GitHubToken.Empty.TokenType));
		});
	}

	[Test]
	public async Task InvalidateTokenTest()
	{
		//Arrange
		GitHubToken? token_BeforeInvalidation, token_AfterInvalidation;
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		await SaveGitHubTokenTest_ValidToken().ConfigureAwait(false);

		token_BeforeInvalidation = await gitHubUserService.GetGitHubToken().ConfigureAwait(false);

		gitHubUserService.InvalidateToken();

		token_AfterInvalidation = await gitHubUserService.GetGitHubToken().ConfigureAwait(false);

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(token_BeforeInvalidation.AccessToken, Is.EqualTo(_token));
			Assert.That(token_BeforeInvalidation.Scope, Is.EqualTo(_scope));
			Assert.That(token_BeforeInvalidation.TokenType, Is.EqualTo(_tokenType));

			Assert.That(token_AfterInvalidation.AccessToken, Is.EqualTo(GitHubToken.Empty.AccessToken));
			Assert.That(token_AfterInvalidation.Scope, Is.EqualTo(GitHubToken.Empty.Scope));
			Assert.That(token_AfterInvalidation.TokenType, Is.EqualTo(GitHubToken.Empty.TokenType));
		});
	}

	[Test]
	public async Task ShouldIncludeOrganizationsTest()
	{
		//Arrange
		bool shouldIncludeOrganizations_Initial, shouldIncludeOrganizations_Final;
		var shouldIncludeOrganizationsChangedTCS = new TaskCompletionSource<bool>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		GitHubUserService.ShouldIncludeOrganizationsChanged += HandleShouldIncludeOrganizationsChanged;

		//Act
		shouldIncludeOrganizations_Initial = gitHubUserService.ShouldIncludeOrganizations;

		gitHubUserService.ShouldIncludeOrganizations = !gitHubUserService.ShouldIncludeOrganizations;
		var shouldIncludeOrganizationsChangedResult = await shouldIncludeOrganizationsChangedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		shouldIncludeOrganizations_Final = gitHubUserService.ShouldIncludeOrganizations;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(shouldIncludeOrganizations_Initial, Is.False);
			Assert.That(shouldIncludeOrganizationsChangedResult);
			Assert.That(shouldIncludeOrganizations_Final);
		});

		void HandleShouldIncludeOrganizationsChanged(object? sender, bool e)
		{
			GitHubUserService.ShouldIncludeOrganizationsChanged -= HandleShouldIncludeOrganizationsChanged;
			shouldIncludeOrganizationsChangedTCS.SetResult(e);
		}
	}
}