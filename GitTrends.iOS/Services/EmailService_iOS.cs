using System;
using System.Threading.Tasks;
using GitTrends.iOS;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(EmailService_iOS))]
namespace GitTrends.iOS
{
    public class EmailService_iOS : IEmailService
    {
        public Task Compose(EmailMessage emailMessage) => throw new NotSupportedException();
    }
}
