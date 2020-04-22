using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GitTrends
{
    public interface IEmailService
    {
        public Task Compose(EmailMessage emailMessage);
    }
}
