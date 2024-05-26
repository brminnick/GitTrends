namespace GitTrends.UnitTests;

public class MockEmail : IEmail
{
	public Task ComposeAsync(EmailMessage? message) => Task.CompletedTask;

	public bool IsComposeSupported { get; } = false;
}