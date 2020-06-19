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

                //Begin fetching the favicons from the website in the background
                var getFavIconsTask = GetFavIcons(baseUrl, cancellationToken);

                //Prioritize GitHub's cached FavIcon
                var gitHubCachedFavIcon = await GetFavIconFromGitHubCache(new Uri(baseUrl), cancellationToken).ConfigureAwait(false);
                if (gitHubCachedFavIcon != null)
                    return gitHubCachedFavIcon.Url;

                var (getAppleTouchIconTask, getShortcutIconTask, getIconTask, getFavIconTask) = await getFavIconsTask.ConfigureAwait(false);

                var appleTouchIconRespnse = await getAppleTouchIconTask.ConfigureAwait(false);
                if (appleTouchIconRespnse != null)
                    return appleTouchIconRespnse.Url;

                var shortcutIconResponse = await getShortcutIconTask.ConfigureAwait(false);
                if (shortcutIconResponse != null)
                    return shortcutIconResponse.Url;

                var iconUrlResponse = await getIconTask.ConfigureAwait(false);
                if (iconUrlResponse != null)
                    return iconUrlResponse.Url;

                var favIconUrlResponse = await getFavIconTask.ConfigureAwait(false);
                if (favIconUrlResponse != null)
                    return favIconUrlResponse.Url;

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

        static async Task<(Task<FavIconResponseModel?> GetAppleTouchIconTask,
                            Task<FavIconResponseModel?> GetShortcutIconTask,
                            Task<FavIconResponseModel?> GetIconTask,
                            Task<FavIconResponseModel?> GetFavIconTask)> GetFavIcons(string baseUrl, CancellationToken cancellationToken)
        {
            var response = await Client.GetAsync(baseUrl, cancellationToken).ConfigureAwait(false);
            var html = await GetHtml(response).ConfigureAwait(false);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var appleTouchIconTask = GetAppleTouchIcon(htmlDocument, baseUrl, cancellationToken);
            var shortcutIconTask = GetShortcutIcon(htmlDocument, baseUrl, cancellationToken);
            var iconTask = GetIcon(htmlDocument, baseUrl, cancellationToken);
            var favIconTask = GetFavIcon(baseUrl, cancellationToken);

            return (appleTouchIconTask, shortcutIconTask, iconTask, favIconTask);
        }

        static async Task<FavIconResponseModel?> GetFavIconFromGitHubCache(Uri uri, CancellationToken cancellationToken)
        {
            try
            {
                var gitHubCacheFavIconUrl = $"https://favicons.githubusercontent.com/{uri.Host}";
                var (isUrlValid, size) = await GetUrlData(gitHubCacheFavIconUrl, cancellationToken).ConfigureAwait(false);

                //The default cahced favicon on GitHub is 874, e.g. https://favicons.githubusercontent.com/google
                if (isUrlValid && size != 874)
                {
                    Debug.WriteLine($"{nameof(GetFavIconFromGitHubCache)}: {gitHubCacheFavIconUrl}, {size}");
                    return new FavIconResponseModel(gitHubCacheFavIconUrl, size);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static async Task<FavIconResponseModel?> GetFavIcon(string url, CancellationToken cancellationToken)
        {
            try
            {
                var faviconUrl = $"{url}favicon.ico";

                var (isUrlValid, size) = await GetUrlData(faviconUrl, cancellationToken).ConfigureAwait(false);

                if (isUrlValid)
                {
                    Debug.WriteLine($"{nameof(GetFavIcon)}: {faviconUrl}, {size}");
                    return new FavIconResponseModel(faviconUrl, size);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static async Task<FavIconResponseModel?> GetShortcutIcon(HtmlDocument htmlDoc, string url, CancellationToken cancellationToken)
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
                    return new FavIconResponseModel(shortcutIconUrl, size);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static async Task<FavIconResponseModel?> GetAppleTouchIcon(HtmlDocument htmlDoc, string url, CancellationToken cancellationToken)
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
                    return new FavIconResponseModel(appleTouchIconUrl, size);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static async Task<FavIconResponseModel?> GetIcon(HtmlDocument htmlDoc, string url, CancellationToken cancellationToken)
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
                    return new FavIconResponseModel(iconUrl, size);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
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

        class FavIconResponseModel
        {
            public FavIconResponseModel(string url, long? contentSize) =>
                (Url, ContentSize) = (url, contentSize);

            public string Url { get; }
            public long? ContentSize { get; }
        }
    }
}