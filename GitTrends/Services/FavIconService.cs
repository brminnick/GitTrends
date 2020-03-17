using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using HtmlAgilityPack;
using Xamarin.Forms;

namespace GitTrends
{
    public class FavIconService
    {
        public const string DefaultFavIcon = "DefaultProfileImage";

        public static async Task<ImageSource> GetFavIconImageSource(string siteUrl)
        {
            try
            {
                var htmlDoc = await new HtmlWeb().LoadFromWebAsync(siteUrl).ConfigureAwait(false);

                var shortcutIconUri = GetShortcutIconUri(htmlDoc, siteUrl);
                var appleTouchIconUri = GetAppleTouchIconUri(htmlDoc, siteUrl);
                var iconUri = GetIconUri(htmlDoc, siteUrl);
                var favIconUri = GetFavIconUri(siteUrl);

                if (appleTouchIconUri != null)
                    return appleTouchIconUri;
                else if (shortcutIconUri != null)
                    return shortcutIconUri;
                else if (iconUri != null)
                    return iconUri;
                else
                    return favIconUri;
            }
            catch (Exception e)
            {
                using var scope = ContainerService.Container.BeginLifetimeScope();
                scope.Resolve<AnalyticsService>().Report(e, new Dictionary<string, string> { { nameof(siteUrl), siteUrl } });

                return DefaultFavIcon;
            }
        }

        static string GetFavIconUri(in string url)
        {
            var faviconUri = $"{url}favicon.ico";

            Debug.WriteLine($"{nameof(GetFavIconUri)}: {faviconUri}");

            return faviconUri;
        }

        static string? GetShortcutIconUri(in HtmlDocument htmlDoc, in string url)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "shortcut icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var shortcutIconUri = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                Debug.WriteLine($"{nameof(GetShortcutIconUri)}: {shortcutIconUri}");

                return shortcutIconUri;
            }
            catch
            {
                return null;
            }
        }

        static string? GetAppleTouchIconUri(in HtmlDocument htmlDoc, in string url)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "apple-touch-icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var appleTouchIconUri = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                Debug.WriteLine($"{nameof(GetAppleTouchIconUri)}: {appleTouchIconUri}");

                return appleTouchIconUri;
            }
            catch
            {
                return null;
            }
        }

        static string? GetIconUri(in HtmlDocument htmlDoc, in string url)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var iconUri = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                Debug.WriteLine($"{nameof(GetIconUri)}: {iconUri}");

                return iconUri;
            }
            catch
            {
                return null;
            }
        }
    }
}
