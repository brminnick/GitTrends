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
            var favIconService = ContainerService.Container.GetService<FavIconService>();

            //Act
            var fileImageSource = (FileImageSource)await favIconService.GetFavIconImageSource(invalidUri, CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(fileImageSource);
            Assert.AreEqual(fileImageSource.File, FavIconService.DefaultFavIcon);
        }

        [TestCase("http://codetraveler.io", "https://codetraveler.io/favicon.ico")] //Clear Text Uri
        [TestCase("https://chrissainty.com/", "https://chrissainty.com/favicon.png")] //Icon Url
        [TestCase("https://duckduckgo.com/", "https://duckduckgo.com/assets/icons/meta/DDG-iOS-icon_60x60.png")] //Apple Touch Icon Url
        [TestCase("https://codetraveler.io", "https://codetraveler.io/favicon.ico")] //FavIcon Url
        public async Task GetFavIconImageSourceTest_ValidUrl(string url, string expectedFavIconUrl)
        {
            //Arrange
            var favIconService = ContainerService.Container.GetService<FavIconService>();

            //Act
            var uriImageSource = (UriImageSource)await favIconService.GetFavIconImageSource(new Uri(url), CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(uriImageSource);
            Assert.AreEqual(uriImageSource.Uri, new Uri(expectedFavIconUrl));
        }

        [TestCase("https://www.google.co.uk", "https://google.co.uk/favicon.ico")]
        public async Task GetFavIconImageSourceTest_CountryCodeTopLevelDomains(string url, string expectedFavIconUrl)
        {
            //Arrange
            var favIconService = ContainerService.Container.GetService<FavIconService>();

            //Act
            var uriImageSource = (UriImageSource)await favIconService.GetFavIconImageSource(new Uri(url), CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(uriImageSource);
            Assert.AreEqual(uriImageSource.Uri, new Uri(expectedFavIconUrl));
        }
    }
}
