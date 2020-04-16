using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using HtmlAgilityPack;
using Xamarin.Forms;

namespace GitTrends
{
    public class FavIconService
    {
        public const string DefaultFavIcon = "DefaultProfileImageGreen";

        static readonly Lazy<HttpClient> _clientHolder = new Lazy<HttpClient>(() => new HttpClient { Timeout = HttpClientTimeout });

        public static TimeSpan HttpClientTimeout { get; } = TimeSpan.FromSeconds(1);

        static HttpClient Client => _clientHolder.Value;

        public static async Task<ImageSource> GetFavIconImageSource(Uri site)
        {
            var baseUrl = $"{site.Scheme}://{GetRootDomain(site.Host)}/";

            try
            {
                var response = await Client.GetAsync(baseUrl).ConfigureAwait(false);

                var html = await GetHtml(response).ConfigureAwait(false);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var appleTouchIconTask = GetAppleTouchIcon(htmlDocument, baseUrl);

                var urlDataSetsTask = Task.WhenAll(appleTouchIconTask,
                                                        GetFavIcon(baseUrl),
                                                        GetShortcutIcon(htmlDocument, baseUrl),
                                                        GetIcon(htmlDocument, baseUrl));

                var (appleTouchIconUrl, _) = await appleTouchIconTask.ConfigureAwait(false);

                //Apple Touch Icons have the largest default resolution
                if (!string.IsNullOrWhiteSpace(appleTouchIconUrl))
                {
                    return appleTouchIconUrl;
                }
                else
                {
                    var urlDataSets = await urlDataSetsTask.ConfigureAwait(false);
                    return getLargestImageUrl(urlDataSets) ?? DefaultFavIcon;
                }
            }
            catch (Exception e)
            {
                using var scope = ContainerService.Container.BeginLifetimeScope();
                scope.Resolve<AnalyticsService>().Report(e, new Dictionary<string, string>
                {
                    { nameof(baseUrl), baseUrl },
                    { nameof(site), site.ToString() }
                });

                return DefaultFavIcon;
            }

            static string? getLargestImageUrl(in IEnumerable<(string? Url, long? Size)> iconDataSet)
            {
                var nonNullDataSet = iconDataSet.Where(x => x.Url != null).OrderByDescending(x => x.Size);

                return nonNullDataSet.Any() ? nonNullDataSet.First().Url : null;
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

        static async Task<(string? FavIconUrl, long? Size)> GetFavIcon(string url)
        {
            try
            {
                var faviconUrl = $"{url}favicon.ico";

                var (isUrlValid, size) = await GetUrlData(faviconUrl).ConfigureAwait(false);

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

        static async Task<(string? ShortcutIconUrl, long? Size)> GetShortcutIcon(HtmlDocument htmlDoc, string url)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "shortcut icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var shortcutIconUrl = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                var (isUrlValid, size) = await GetUrlData(shortcutIconUrl).ConfigureAwait(false);

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

        static async Task<(string? AppleTouchIconUrl, long? size)> GetAppleTouchIcon(HtmlDocument htmlDoc, string url)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "apple-touch-icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var appleTouchIconUrl = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                var (isUrlValid, size) = await GetUrlData(appleTouchIconUrl).ConfigureAwait(false);

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

        static async Task<(string? IconUrl, long? size)> GetIcon(HtmlDocument htmlDoc, string url)
        {
            try
            {
                var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(x => x.Attributes.Where(x => x.Value is "icon")).First();
                var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(x => x.Name is "href").Value;

                var iconUrl = hrefValue.Contains("http") ? hrefValue : url.Trim('/') + hrefValue;

                var (isUrlValid, size) = await GetUrlData(iconUrl).ConfigureAwait(false);

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

        static async ValueTask<(bool IsUrlValid, long? size)> GetUrlData(string? url)
        {
            try
            {
                if (url is null || url.EndsWith(".svg"))
                    return (false, null);

                var response = await Client.GetAsync(url).ConfigureAwait(false);

                return (response.IsSuccessStatusCode, response.Content.Headers.ContentLength);
            }
            catch
            {
                return (false, null);
            }
        }
    }
}