using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using GitTrends.Shared;
using HtmlAgilityPack;
using Xamarin.Forms;

namespace GitTrends
{
    public class FavIconService
    {
        static readonly Lazy<HttpClient> _clientHolder = new Lazy<HttpClient>(new HttpClient { Timeout = TimeSpan.FromSeconds(10) });
        readonly IAnalyticsService _analyticsService;

        public FavIconService(IAnalyticsService analyticsService) => _analyticsService = analyticsService;

        public static string DefaultFavIcon => BaseTheme.GetDefaultReferringSiteImageSource();

        static HttpClient Client => _clientHolder.Value;

        public async Task<ImageSource> GetFavIconImageSource(Uri site, CancellationToken cancellationToken)
        {
            var scheme = site.Scheme is "http" ? "https" : site.Scheme;

            string baseUrl = string.Empty;

            try
            {
                baseUrl = $"{scheme}://{GetRootDomain(site.Host)}/";
                var response = await Client.GetAsync(baseUrl, cancellationToken).ConfigureAwait(false);

                var html = await GetHtml(response).ConfigureAwait(false);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var appleTouchIconTask = GetAppleTouchIcon(htmlDocument, baseUrl, cancellationToken);
                var shortcutIconTask = GetShortcutIcon(htmlDocument, baseUrl, cancellationToken);
                var iconTask = GetIcon(htmlDocument, baseUrl, cancellationToken);
                var favIconTask = GetFavIcon(baseUrl, cancellationToken);

                var (appleTouchIconUrl, _) = await appleTouchIconTask.ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(appleTouchIconUrl))
                    return appleTouchIconUrl;

                var (shortcutIconUrl, _) = await shortcutIconTask.ConfigureAwait(false);
                if (shortcutIconUrl != null)
                    return shortcutIconUrl;

                var (iconUrl, _) = await iconTask.ConfigureAwait(false);
                if (iconUrl != null)
                    return iconUrl;

                var (favIconUrl, _) = await favIconTask.ConfigureAwait(false);
                if (favIconUrl != null)
                    return favIconUrl;

                return DefaultFavIcon;
            }
            catch (Exception e)
            {
                _analyticsService.Report(e, new Dictionary<string, string>
                {
                    { nameof(baseUrl), baseUrl },
                    { nameof(site), site.ToString() }
                });

                return DefaultFavIcon;
            }
        }

        //https://stackoverflow.com/a/35213737/5953643
        static string GetRootDomain(in string host)
        {
            string[] domains = host.Split('.');

            if (domains.Length >= 3)
            {
                int domainCount = domains.Length;
                // handle international country code TLDs 
                // www.amazon.co.uk => amazon.co.uk
                if (domains[domainCount - 1].Length < 3 && domains[domainCount - 2].Length <= 3)
                    return string.Join(".", domains, domainCount - 3, 3);
                else
                    return string.Join(".", domains, domainCount - 2, 2);
            }
            else
            {
                return host;
            }
        }

        static async Task<string> GetHtml(HttpResponseMessage response)
        {
            try
            {
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                //Work-around for UTF-8 https://stackoverflow.com/a/50355945/5953643
                using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync(), Encoding.GetEncoding("iso-8859-1"));
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        static async Task<(string? FavIconUrl, long? ContentSize)> GetFavIcon(string url, CancellationToken cancellationToken)
        {
            try
            {
                var faviconUrl = $"{url}favicon.ico";

                var (isUrlValid, size) = await GetUrlData(faviconUrl, cancellationToken).ConfigureAwait(false);

                if (isUrlValid)
                {
                    Debug.WriteLine($"{nameof(GetFavIcon)}: {faviconUrl}, {size}");
                    return (faviconUrl, size);
                }
                else
                {
                    return (null, null);
                }
            }
            catch
            {
                return (null, null);
            }
        }

        static async Task<(string? ShortcutIconUrl, long? ContentSize)> GetShortcutIcon(HtmlDocument htmlDoc, string url, CancellationToken cancellationToken)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "shortcut icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var shortcutIconUrl = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                var (isUrlValid, size) = await GetUrlData(shortcutIconUrl, cancellationToken).ConfigureAwait(false);

                if (isUrlValid)
                {
                    Debug.WriteLine($"{nameof(GetIcon)}: {shortcutIconUrl}, {size}");
                    return (shortcutIconUrl, size);
                }
                else
                {
                    return (null, null);
                }
            }
            catch
            {
                return (null, null);
            }
        }

        static async Task<(string? AppleTouchIconUrl, long? ContentSize)> GetAppleTouchIcon(HtmlDocument htmlDoc, string url, CancellationToken cancellationToken)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "apple-touch-icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var appleTouchIconUrl = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                var (isUrlValid, size) = await GetUrlData(appleTouchIconUrl, cancellationToken).ConfigureAwait(false);

                if (isUrlValid)
                {
                    Debug.WriteLine($"{nameof(GetAppleTouchIcon)}: {appleTouchIconUrl}, {size}");
                    return (appleTouchIconUrl, size);
                }
                else
                {
                    return (null, null);
                }
            }
            catch
            {
                return (null, null);
            }
        }

        static async Task<(string? IconUrl, long? ContentSize)> GetIcon(HtmlDocument htmlDoc, string url, CancellationToken cancellationToken)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var iconUrl = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                var (isUrlValid, size) = await GetUrlData(iconUrl, cancellationToken).ConfigureAwait(false);

                if (isUrlValid)
                {
                    Debug.WriteLine($"{nameof(GetIcon)}: {iconUrl}, {size}");
                    return (iconUrl, size);
                }
                else
                {
                    return (null, null);
                }
            }
            catch
            {
                return (null, null);
            }
        }

        static async ValueTask<(bool IsUrlValid, long? ContentSize)> GetUrlData(string? url, CancellationToken cancellationToken)
        {
            try
            {
                if (url is null || url.EndsWith(".svg"))
                    return (false, null);

                var response = await Client.GetAsync(url, cancellationToken).ConfigureAwait(false);

                return (response.IsSuccessStatusCode, response.Content.Headers.ContentLength);
            }
            catch
            {
                return (false, null);
            }
        }
    }
}