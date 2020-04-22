using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Text;
using GitTrends.Droid;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(EmailService_Android))]
namespace GitTrends.Droid
{
    public class EmailService_Android : IEmailService
    {
        public Task Compose(EmailMessage message)
        {
            var intent = CreateIntent(message);
            var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                flags |= ActivityFlags.LaunchAdjacent;

            intent.SetFlags(flags);

            Platform.AppContext.StartActivity(intent);

            return Task.FromResult(true);
        }

        static Intent CreateIntent(EmailMessage message)
        {
            var action = (message?.Attachments?.Count ?? 0) switch
            {
                0 => Intent.ActionSendto,
                1 => Intent.ActionSend,
                _ => Intent.ActionSendMultiple
            };
            var intent = new Intent(action);

            if (action == Intent.ActionSendto)
                intent.SetData(Android.Net.Uri.Parse("mailto:"));
            else
                intent.SetType("message/rfc822");

            if (!string.IsNullOrEmpty(message?.Body))
            {
                if (message.BodyFormat == EmailBodyFormat.Html)
                {
                    ISpanned html;

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        html = Html.FromHtml(message.Body, FromHtmlOptions.ModeLegacy);
                    }
                    else
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        html = Html.FromHtml(message.Body);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    intent.PutExtra(Intent.ExtraText, html);
                }
                else
                {
                    intent.PutExtra(Intent.ExtraText, message.Body);
                }
            }
            if (!string.IsNullOrEmpty(message?.Subject))
                intent.PutExtra(Intent.ExtraSubject, message.Subject);
            if (message?.To?.Count > 0)
                intent.PutExtra(Intent.ExtraEmail, message.To.ToArray());
            if (message?.Cc?.Count > 0)
                intent.PutExtra(Intent.ExtraCc, message.Cc.ToArray());
            if (message?.Bcc?.Count > 0)
                intent.PutExtra(Intent.ExtraBcc, message.Bcc.ToArray());

            if (message?.Attachments?.Count > 0)
            {
                throw new NotSupportedException();
            }

            return intent;
        }
    }
}
