using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class LanguageServiceTests : BaseTest
    {
        [Test]
        public async Task ConfirmLanguageChanges_Initialize()
        {
            //Arrange
            CultureInfo? currentCulture_Initial, currentUICulture_Initial, currentCulture_Final, currentUICulture_Final;
            bool didPreferredLanguageChangeFire = false;

            var preferredLanguageChangedTCS = new TaskCompletionSource<string>();
            LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

            var languageService = ServiceCollection.ServiceProvider.GetService<LanguageService>();

            //Act
            currentCulture_Initial = CultureInfo.DefaultThreadCurrentCulture;
            currentUICulture_Initial = CultureInfo.DefaultThreadCurrentUICulture;

            languageService.Initialize();
            var preferredLanguageChangedResult = await preferredLanguageChangedTCS.Task.ConfigureAwait(false);

            currentCulture_Final = CultureInfo.DefaultThreadCurrentCulture;
            currentUICulture_Final = CultureInfo.DefaultThreadCurrentUICulture;

            //Assert
            Assert.IsTrue(didPreferredLanguageChangeFire);

            Assert.IsNull(currentCulture_Initial);
            Assert.IsNull(currentUICulture_Initial);

            Assert.IsNotNull(currentCulture_Final);
            Assert.IsNotNull(currentUICulture_Final);

            Assert.AreNotEqual(currentCulture_Final, currentCulture_Initial);
            Assert.AreNotEqual(currentUICulture_Final, currentUICulture_Initial);

            Assert.AreEqual(currentCulture_Initial, currentUICulture_Initial);
            Assert.AreEqual(currentCulture_Final, currentUICulture_Final);

            Assert.Contains(currentCulture_Final?.Name, CultureConstants.CulturePickerOptions.Keys.ToList());
            Assert.Contains(currentUICulture_Final?.Name, CultureConstants.CulturePickerOptions.Keys.ToList());

            Assert.AreEqual(currentCulture_Final?.Name, preferredLanguageChangedResult);
            Assert.AreEqual(currentUICulture_Final?.Name, preferredLanguageChangedResult);

            void HandlePreferredLanguageChanged(object? sender, string e)
            {
                LanguageService.PreferredLanguageChanged -= HandlePreferredLanguageChanged;

                didPreferredLanguageChangeFire = true;
                preferredLanguageChangedTCS.SetResult(e);
            }
        }

        [Test]
        public async Task ConfirmLanguageChanges_SetPreferedLanguage()
        {
            //Arrange
            CultureInfo? currentCulture_Initial, currentUICulture_Initial, currentCulture_Final, currentUICulture_Final;

            var availableLanguages = CultureConstants.CulturePickerOptions.Keys.ToList();
            var languageService = ServiceCollection.ServiceProvider.GetService<LanguageService>();

            foreach (var language in availableLanguages)
            {
                bool didPreferredLanguageChangeFire = false;
                var preferredLanguageChangedTCS = new TaskCompletionSource<string>();

                LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

                //Act
                currentCulture_Initial = CultureInfo.DefaultThreadCurrentCulture;
                currentUICulture_Initial = CultureInfo.DefaultThreadCurrentUICulture;

                languageService.PreferredLanguage = language;
                var preferredLanguageChangedResult = await preferredLanguageChangedTCS.Task.ConfigureAwait(false);

                currentCulture_Final = CultureInfo.DefaultThreadCurrentCulture;
                currentUICulture_Final = CultureInfo.DefaultThreadCurrentUICulture;

                //Assert
                Assert.IsTrue(didPreferredLanguageChangeFire);
                Assert.IsNotNull(currentCulture_Final);
                Assert.IsNotNull(currentUICulture_Final);

                Assert.AreNotEqual(currentCulture_Final, currentCulture_Initial);
                Assert.AreNotEqual(currentUICulture_Final, currentUICulture_Initial);

                Assert.AreEqual(currentCulture_Initial, currentUICulture_Initial);
                Assert.AreEqual(currentCulture_Final, currentUICulture_Final);

                Assert.AreEqual(language, currentCulture_Final?.Name);
                Assert.AreEqual(language, currentUICulture_Final?.Name);

                Assert.AreEqual(preferredLanguageChangedResult, language);
                Assert.AreEqual(preferredLanguageChangedResult, currentCulture_Final?.Name);
                Assert.AreEqual(preferredLanguageChangedResult, currentUICulture_Final?.Name);

                void HandlePreferredLanguageChanged(object? sender, string e)
                {
                    LanguageService.PreferredLanguageChanged -= HandlePreferredLanguageChanged;

                    didPreferredLanguageChangeFire = true;
                    preferredLanguageChangedTCS.SetResult(e);
                }
            }
        }

        [TestCase("en-US")]
        [TestCase("ps")]
        [TestCase("prs")]
        public void ConfirmError_SetPreferedLanguage(string language)
        {
            //Ensure test case is not supported by GitTrends
            Assert.IsFalse(CultureConstants.CulturePickerOptions.Keys.ToList().Contains(language));

            //Arrange
            var availableLanguages = CultureConstants.CulturePickerOptions.Keys.ToList();
            var languageService = ServiceCollection.ServiceProvider.GetService<LanguageService>();

            bool didPreferredLanguageChangeFire = false;
            LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

            //Act //Assert
            try
            {
                Assert.Throws<ArgumentException>(() => languageService.PreferredLanguage = language);

                //Assert
                Assert.IsFalse(didPreferredLanguageChangeFire);
            }
            finally
            {
                LanguageService.PreferredLanguageChanged -= HandlePreferredLanguageChanged;
            }

            void HandlePreferredLanguageChanged(object? sender, string e) => didPreferredLanguageChangeFire = true;
        }
    }
}
