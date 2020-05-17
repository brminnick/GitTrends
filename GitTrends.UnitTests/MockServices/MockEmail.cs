using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    public class MockEmail : IEmail
    {
        public Task ComposeAsync() => ComposeAsync(new EmailMessage());

        public Task ComposeAsync(string subject, string body, params string[] to) => ComposeAsync(new EmailMessage(subject, body, to));

        public Task ComposeAsync(EmailMessage message) => Task.CompletedTask;
    }
}
