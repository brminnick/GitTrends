using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.XPath;
using AsyncAwaitBestPractices;
using FFImageLoading.Work;
using GitTrends.Shared;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace GitTrends
{
    public class ReferringSiteModel : BaseRepositoryModel
    {
        public const int FavIconSize = 32;

        public ReferringSiteModel(long count, long uniques, string referrer) : base(count, uniques)
        {
            Referrer = referrer;
            Uri.TryCreate("https://" + referrer, UriKind.Absolute, out var referringUri);

            if (referringUri != null && !referringUri.ToString().Contains('.'))
            {
                ReferrerUri = new Uri(referringUri.ToString().TrimEnd('/') + ".com/");
                IsReferrerUriValid = true;
            }
            else if (referringUri != null)
            {
                ReferrerUri = referringUri;
                IsReferrerUriValid = true;
            }
            else
            {
                ReferrerUri = null;
                IsReferrerUriValid = false;
            }
        }

        [JsonProperty("referrer")]
        public string Referrer { get; }

        [JsonProperty("isReferrerUriValid")]
        public bool IsReferrerUriValid { get; }

        [JsonProperty("referrerUri")]
        public Uri? ReferrerUri { get; }

        [JsonProperty("favIcon")]
        public Xamarin.Forms.ImageSource? FavIcon { get; set; }
    }
}
