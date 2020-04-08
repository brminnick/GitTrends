using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using HtmlAgilityPack;
using Xamarin.Forms;

namespace GitTrends
{
    public class FavIconService
    {
        public const string DefaultFavIcon = "DefaultProfileImageGreen";

        static readonly Lazy<HttpClient> _clientHolder = new Lazy<HttpClient>(new HttpClient { Timeout = TimeSpan.FromSeconds(1) });

        static HttpClient Client => _clientHolder.Value;

        public static async Task<ImageSource> GetFavIconImageSource(string siteUrl)
        {
            try
            {
                var htmlDoc = await new HtmlWeb().LoadFromWebAsync(siteUrl).ConfigureAwait(false);

                var (shortcutIconUrl, appleTouchIconUrl, iconUrl, favIconUrl) = await GetFavIcons(htmlDoc, siteUrl);

                if (appleTouchIconUrl != null)
                    return appleTouchIconUrl;
                else if (shortcutIconUrl != null)
                    return shortcutIconUrl;
                else if (iconUrl != null)
                    return iconUrl;
                else if (favIconUrl != null)
                    return favIconUrl;
                else
                    return DefaultFavIcon;
            }
            catch (Exception e)
            {
                using var scope = ContainerService.Container.BeginLifetimeScope();
                scope.Resolve<AnalyticsService>().Report(e, new Dictionary<string, string> { { nameof(siteUrl), siteUrl } });

                return DefaultFavIcon;
            }
        }

        static async Task<(string? ShortcutIconUrl, string? AppleTouchIconUrl, string? IconUrl, string? FavIconUrl)> GetFavIcons(HtmlDocument htmlDoc, string siteUrl)
        {
            var shortcutIconUrlTask = GetShortcutIconUrl(htmlDoc, siteUrl);
            var appleTouchIconUrlTask = GetAppleTouchIconUrl(htmlDoc, siteUrl);
            var iconUrlTask = GetIconUrl(htmlDoc, siteUrl);
            var favIconUrlTask = GetFavIconUrl(siteUrl);

            await Task.WhenAll(shortcutIconUrlTask, appleTouchIconUrlTask, iconUrlTask, favIconUrlTask).ConfigureAwait(false);

            var shortcutIconUrl = await shortcutIconUrlTask.ConfigureAwait(false);
            var appleTouchIconUrl = await appleTouchIconUrlTask.ConfigureAwait(false);
            var iconUrl = await iconUrlTask.ConfigureAwait(false);
            var favIconUrl = await favIconUrlTask.ConfigureAwait(false);

            return (shortcutIconUrl, appleTouchIconUrl, iconUrl, favIconUrl);
        }

        static async Task<string?> GetFavIconUrl(string url)
        {
            var faviconUrl = $"{url}favicon.ico";

            var isValid = await IsUrlValid(faviconUrl).ConfigureAwait(false);

            if (isValid)
            {
                Debug.WriteLine($"{nameof(GetIconUrl)}: {faviconUrl}");
                return faviconUrl;
            }
            else
            {
                Debug.WriteLine($"{nameof(GetIconUrl)}: null");
                return null;
            }
        }

        static async Task<string?> GetShortcutIconUrl(HtmlDocument htmlDoc, string url)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "shortcut icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var shortcutIconUrl = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                var isValid = await IsUrlValid(shortcutIconUrl).ConfigureAwait(false);
                if (isValid)
                {
                    Debug.WriteLine($"{nameof(GetIconUrl)}: {shortcutIconUrl}");
                    return shortcutIconUrl;
                }
                else
                {
                    Debug.WriteLine($"{nameof(GetIconUrl)}: null");
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static async Task<string?> GetAppleTouchIconUrl(HtmlDocument htmlDoc, string url)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "apple-touch-icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var appleTouchIconUrl = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                var isValid = await IsUrlValid(appleTouchIconUrl).ConfigureAwait(false);
                if (isValid)
                {
                    Debug.WriteLine($"{nameof(GetIconUrl)}: {appleTouchIconUrl}");
                    return appleTouchIconUrl;
                }
                else
                {
                    Debug.WriteLine($"{nameof(GetIconUrl)}: null");
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static async Task<string?> GetIconUrl(HtmlDocument htmlDoc, string url)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var iconUrl = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                var isValid = await IsUrlValid(iconUrl).ConfigureAwait(false);
                if (isValid)
                {
                    Debug.WriteLine($"{nameof(GetIconUrl)}: {iconUrl}");
                    return iconUrl;
                }
                else
                {
                    Debug.WriteLine($"{nameof(GetIconUrl)}: null");
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static async ValueTask<bool> IsUrlValid(string? url)
        {
            if (url is null || url.EndsWith(".svg"))
                return false;

            var response = await Client.GetAsync(url).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
    }
}
