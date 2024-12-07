using System.Diagnostics;
using System.Text;
using GitTrends.Common;
using HtmlAgilityPack;

namespace GitTrends;

public class FavIconService(IAnalyticsService analyticsService, HttpClient client)
{
	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly HttpClient _client = client;

	public static string DefaultFavIcon => BaseTheme.GetDefaultReferringSiteImageSource();

	public async Task<ImageSource> GetFavIconImageSource(Uri site, CancellationToken cancellationToken)
	{
		var scheme = site.Scheme == Uri.UriSchemeHttp ? Uri.UriSchemeHttps : site.Scheme;

		string baseUrl = string.Empty;

		try
		{
			baseUrl = $"{scheme}://{GetRootDomain(site.Host)}/";

			var (getAppleTouchIconTask, getShortcutIconTask, getIconTask, getFavIconTask) = await GetFavIcons(baseUrl, cancellationToken).ConfigureAwait(false);

			var appleTouchIconResponse = await getAppleTouchIconTask.ConfigureAwait(false);
			if (appleTouchIconResponse != null)
				return appleTouchIconResponse.Url;

			var shortcutIconResponse = await getShortcutIconTask.ConfigureAwait(false);
			if (shortcutIconResponse is not null)
				return shortcutIconResponse.Url;

			var iconUrlResponse = await getIconTask.ConfigureAwait(false);
			if (iconUrlResponse is not null)
				return iconUrlResponse.Url;

			var favIconUrlResponse = await getFavIconTask.ConfigureAwait(false);
			if (favIconUrlResponse is not null)
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

	[Obsolete("GitHub has deprecated their FavIcon Cache")]
	async Task<FavIconResponseModel?> GetFavIconFromGitHubCache(Uri uri, CancellationToken cancellationToken)
	{
		try
		{
			var gitHubCacheFavIconUrl = $"https://favicons.githubusercontent.com/{uri.Host}";
			var (isUrlValid, size) = await GetUrlData(gitHubCacheFavIconUrl, cancellationToken).ConfigureAwait(false);

			//The default cached favicon on GitHub is 874, e.g. https://favicons.githubusercontent.com/google
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

	async Task<FavIconResponseModel?> GetFavIcon(string url, CancellationToken cancellationToken)
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

	async Task<FavIconResponseModel?> GetShortcutIcon(HtmlDocument htmlDoc, string url, CancellationToken cancellationToken)
	{
		try
		{
			var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(static x => x.Attributes.Where(static x => x.Value is "shortcut icon")).First();
			var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(static x => x.Name is "href").Value;

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

	async Task<FavIconResponseModel?> GetAppleTouchIcon(HtmlDocument htmlDoc, string url, CancellationToken cancellationToken)
	{
		try
		{
			var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(static x => x.Attributes.Where(static x => x.Value is "apple-touch-icon")).First();
			var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(static x => x.Name is "href").Value;

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

	async Task<FavIconResponseModel?> GetIcon(HtmlDocument htmlDoc, string url, CancellationToken cancellationToken)
	{
		try
		{
			var shortcutIconNode = htmlDoc.DocumentNode.SelectNodes("//head//link").SelectMany(static x => x.Attributes.Where(static x => x.Value is "icon")).First();
			var hrefValue = shortcutIconNode.OwnerNode.Attributes.First(static x => x.Name is "href").Value;

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

	async ValueTask<(bool IsUrlValid, long? ContentSize)> GetUrlData(string? url, CancellationToken cancellationToken)
	{
		try
		{
			if (url is null || url.EndsWith(".svg"))
				return (false, null);

			var response = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);

			return (response.IsSuccessStatusCode, response.Content.Headers.ContentLength);
		}
		catch
		{
			return (false, null);
		}
	}

	async Task<(Task<FavIconResponseModel?> GetAppleTouchIconTask,
					Task<FavIconResponseModel?> GetShortcutIconTask,
					Task<FavIconResponseModel?> GetIconTask,
					Task<FavIconResponseModel?> GetFavIconTask)> GetFavIcons(string baseUrl, CancellationToken cancellationToken)
	{
		var response = await _client.GetAsync(baseUrl, cancellationToken).ConfigureAwait(false);
		var html = await GetHtml(response).ConfigureAwait(false);

		var htmlDocument = new HtmlDocument();
		htmlDocument.LoadHtml(html);

		var appleTouchIconTask = GetAppleTouchIcon(htmlDocument, baseUrl, cancellationToken);
		var shortcutIconTask = GetShortcutIcon(htmlDocument, baseUrl, cancellationToken);
		var iconTask = GetIcon(htmlDocument, baseUrl, cancellationToken);
		var favIconTask = GetFavIcon(baseUrl, cancellationToken);

		return (appleTouchIconTask, shortcutIconTask, iconTask, favIconTask);
	}

	class FavIconResponseModel(string url, long? contentSize)
	{
		public string Url { get; } = url;
		public long? ContentSize { get; } = contentSize;
	}
}