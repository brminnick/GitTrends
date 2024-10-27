using System.Web;
using GitTrends.Common;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends.UnitTests;

class GitHubAuthenticationServiceTests : BaseTest
{
	[Test]
	public async Task AuthorizeSessionStartedTest()
	{
		//Arrange
		var didAuthorizeSessionStartedFire = false;
		var authorizeSessionStartedTCS = new TaskCompletionSource();

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		GitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;

		//Act
		Assert.ThrowsAsync<Exception>(async () => await gitHubAuthenticationService.AuthorizeSession(new Uri("https://google.com"), CancellationToken.None).ConfigureAwait(false));
		await authorizeSessionStartedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.That(didAuthorizeSessionStartedFire, Is.True);

		void HandleAuthorizeSessionStarted(object? sender, EventArgs e)
		{
			GitHubAuthenticationService.AuthorizeSessionStarted -= HandleAuthorizeSessionStarted;
			didAuthorizeSessionStartedFire = true;
			authorizeSessionStartedTCS.SetResult();
		}
	}

	[Test]
	public async Task AuthorizeSessionCompletedTest()
	{
		//Arrange
		bool isAuthorizationSuccessful;

		var didAuthorizeSessionCompletedFire = false;
		var authorizeSessionCompletedTCS = new TaskCompletionSource<bool>();

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

		//Act
		Assert.ThrowsAsync<Exception>(async () => await gitHubAuthenticationService.AuthorizeSession(new Uri("https://google.com"), CancellationToken.None).ConfigureAwait(false));
		isAuthorizationSuccessful = await authorizeSessionCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(didAuthorizeSessionCompletedFire, Is.True);
			Assert.That(isAuthorizationSuccessful, Is.False);
		});

		void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e)
		{
			GitHubAuthenticationService.AuthorizeSessionCompleted -= HandleAuthorizeSessionCompleted;
			didAuthorizeSessionCompletedFire = true;
			authorizeSessionCompletedTCS.SetResult(e.IsSessionAuthorized);
		}
	}

	[Test]
	public async Task ActivateDemoUserTest()
	{
		//Arrange
		var didDemoUserActivatedFire = false;
		var demoUserActivatedTCS = new TaskCompletionSource();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;

		//Act
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);
		await demoUserActivatedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didDemoUserActivatedFire);
			Assert.That(gitHubUserService.Alias, Is.EqualTo(DemoUserConstants.Alias));
			Assert.That(gitHubUserService.Name, Is.EqualTo(DemoUserConstants.Name));
		});

		void HandleDemoUserActivated(object? sender, EventArgs e)
		{
			GitHubAuthenticationService.DemoUserActivated -= HandleDemoUserActivated;
			didDemoUserActivatedFire = true;
			demoUserActivatedTCS.SetResult();
		}
	}

	[Test]
	public async Task LogOutTest()
	{
		//Arrange
		string gitHubUserName_Initial, gitHubUserName_Final, gitHubUserAlias_Initial, gitHubUserAlias_Final;

		var didDLoggedOutFire = false;
		var loggedOutTCS = new TaskCompletionSource();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);
		GitHubAuthenticationService.LoggedOut += HandleLoggedOut;

		//Act
		gitHubUserName_Initial = gitHubUserService.Name;
		gitHubUserAlias_Initial = gitHubUserService.Alias;

		await gitHubAuthenticationService.LogOut(TestCancellationTokenSource.Token).ConfigureAwait(false);
		await loggedOutTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		gitHubUserAlias_Final = gitHubUserService.Alias;
		gitHubUserName_Final = gitHubUserService.Name;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didDLoggedOutFire, Is.True);
			Assert.That(gitHubUserName_Initial, Is.EqualTo(DemoUserConstants.Name));
			Assert.That(gitHubUserAlias_Initial, Is.EqualTo(DemoUserConstants.Alias));
		});

		void HandleLoggedOut(object? sender, EventArgs e)
		{
			GitHubAuthenticationService.LoggedOut -= HandleLoggedOut;
			didDLoggedOutFire = true;
			loggedOutTCS.SetResult();
		}
	}

	[Test]
	public void GetGitHubLoginUrlTest()
	{
		//Arrange
		string gitHubLoginUrl, scope, state;
		var preferences = ServiceCollection.ServiceProvider.GetRequiredService<IPreferences>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		//Act
		gitHubLoginUrl = gitHubAuthenticationService.GetGitHubLoginUrl();

		scope = HttpUtility.ParseQueryString(gitHubLoginUrl).Get(nameof(scope)) ?? throw new InvalidOperationException();
		state = HttpUtility.ParseQueryString(gitHubLoginUrl).Get(nameof(state)) ?? throw new InvalidOperationException();

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(gitHubLoginUrl, Is.Not.Null);
			Assert.That(gitHubLoginUrl, Is.Not.Empty);

			Assert.That(scope, Is.Not.Null);
			Assert.That(scope, Is.Not.Empty);
			Assert.That(scope, Is.EqualTo(HttpUtility.UrlDecode(GitHubConstants.OAuthScope)));

			Assert.That(state, Is.Not.Null);
			Assert.That(state, Is.Not.Empty);
			Assert.That(state, Is.EqualTo(preferences.Get("MostRecentSessionId", string.Empty)));
		});
	}

	[Test, Ignore("ToDo")]
	public void AuthorizeSessionTest()
	{
		throw new NotImplementedException();
	}
}