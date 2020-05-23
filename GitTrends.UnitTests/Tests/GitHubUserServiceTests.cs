using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class GitHubUserServiceTests : BaseTest
    {
        const string _scope = GitHubConstants.OAuthScope;
        const string _tokenType = "Bearer";

        readonly string _token = Guid.NewGuid().ToString();

        [Test]
        public async Task SaveGitHubTokenTest()
        {
            //Arrange
            var gitHubToken = new GitHubToken(_token, _scope, _tokenType);
            var gitHubUserService = ContainerService.Container.GetService<GitHubUserService>();

            //Act
            await gitHubUserService.SaveGitHubToken(gitHubToken).ConfigureAwait(false);
            var retrievedToken = await gitHubUserService.GetGitHubToken().ConfigureAwait(false);

            //Assert
            Assert.AreEqual(_token, retrievedToken.AccessToken);
            Assert.AreEqual(_scope, retrievedToken.Scope);
            Assert.AreEqual(_tokenType, retrievedToken.TokenType);

            Assert.AreEqual(gitHubToken.AccessToken, retrievedToken.AccessToken);
            Assert.AreEqual(gitHubToken.Scope, retrievedToken.Scope);
            Assert.AreEqual(gitHubToken.TokenType, retrievedToken.TokenType);
        }

        [Test]
        public async Task InvalidateTokenTest()
        {
            //Arrange
            GitHubToken? token_BeforeInvalidation, token_AfterInvalidation;
            var gitHubUserService = ContainerService.Container.GetService<GitHubUserService>();

            //Act
            await SaveGitHubTokenTest().ConfigureAwait(false);

            token_BeforeInvalidation = await gitHubUserService.GetGitHubToken().ConfigureAwait(false);

            gitHubUserService.InvalidateToken();

            token_AfterInvalidation = await gitHubUserService.GetGitHubToken().ConfigureAwait(false);

            //Assert
            Assert.AreEqual(_token, token_BeforeInvalidation.AccessToken);
            Assert.AreEqual(_scope, token_BeforeInvalidation.Scope);
            Assert.AreEqual(_tokenType, token_BeforeInvalidation.TokenType);

            Assert.AreEqual(GitHubToken.Empty.AccessToken, token_AfterInvalidation.AccessToken);
            Assert.AreEqual(GitHubToken.Empty.Scope, token_AfterInvalidation.Scope);
            Assert.AreEqual(GitHubToken.Empty.TokenType, token_AfterInvalidation.TokenType);
        }
    }
}
