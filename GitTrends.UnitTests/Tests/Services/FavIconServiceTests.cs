using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Forms;

namespace GitTrends.UnitTests
{
    class FavIconServiceTests : BaseTest
    {
        [Test]
        public async Task GetFavIconImageSourceTest_InvalidUri()
        {
            //Arrange
            Uri invalidUri = new Uri("https://abc123456789.com/");
            var favIconService = ServiceCollection.ServiceProvider.GetRequiredService<FavIconService>();

            //Act
            var fileImageSource = (FileImageSource)await favIconService.GetFavIconImageSource(invalidUri, CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(fileImageSource);
            Assert.AreEqual(fileImageSource.File, FavIconService.DefaultFavIcon);
        }

        [TestCase("http://codetraveler.io", "https://favicons.githubusercontent.com/codetraveler.io")] //Clear Text Uri
        [TestCase("https://codetraveler.io", "https://favicons.githubusercontent.com/codetraveler.io")] //Cached Google FavIcon
        [TestCase("https://contiva.atlassian.net/", "https://wac-cdn.atlassian.com/assets/img/favicons/atlassian/favicon.png")] //Icon Url
        [TestCase("https://chrissainty.com/", "https://chrissainty.com/favicon.png")] //Shortcut icon Url
        [TestCase("https://forums.xamarin.com/", "https://xamarin.com/static/images/tiles/apple-touch-icon.png")] //Apple Touch Icon Url
        [TestCase("https://javiersuarezruiz.wordpress.com/", "https://wordpress.com/favicon.ico")] //FavIcon Url
        public async Task GetFavIconImageSourceTest_ValidUrl(string url, string expectedFavIconUrl)
        {
            //Arrange
            var favIconService = ServiceCollection.ServiceProvider.GetRequiredService<FavIconService>();

            //Act
            var uriImageSource = (UriImageSource)await favIconService.GetFavIconImageSource(new Uri(url), CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(uriImageSource);
            Assert.AreEqual(uriImageSource.Uri, new Uri(expectedFavIconUrl));
        }

        [TestCase("https://www.abbotslangley-pc.gov.uk", "https://www.abbotslangley-pc.gov.uk/wp-content/uploads/2017/09/favicon-1.png")]
        public async Task GetFavIconImageSourceTest_CountryCodeTopLevelDomains(string url, string expectedFavIconUrl)
        {
            //Arrange
            var favIconService = ServiceCollection.ServiceProvider.GetRequiredService<FavIconService>();

            //Act
            var uriImageSource = (UriImageSource)await favIconService.GetFavIconImageSource(new Uri(url), CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(uriImageSource);
            Assert.AreEqual(new Uri(expectedFavIconUrl), uriImageSource.Uri);
            Assert.IsFalse(uriImageSource.Uri.ToString().Contains("https://favicons.githubusercontent.com"));
        }
    }
}
