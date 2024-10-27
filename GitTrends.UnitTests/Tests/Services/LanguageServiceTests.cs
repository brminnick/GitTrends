using System.Globalization;
using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

class LanguageServiceTests : BaseTest
{
	[Test]
	public void ConfirmLanguageChanges_Initialize()
	{
		//Arrange
		CultureInfo? currentCulture_Initial, currentUICulture_Initial, currentCulture_Final, currentUICulture_Final;
		bool didPreferredLanguageChangeFire = false;

		LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

		var languageService = ServiceCollection.ServiceProvider.GetRequiredService<LanguageService>();

		//Act
		try
		{
			currentCulture_Initial = CultureInfo.DefaultThreadCurrentCulture;
			currentUICulture_Initial = CultureInfo.DefaultThreadCurrentUICulture;

			languageService.Initialize();

			currentCulture_Final = CultureInfo.DefaultThreadCurrentCulture;
			currentUICulture_Final = CultureInfo.DefaultThreadCurrentUICulture;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(didPreferredLanguageChangeFire, Is.False);

				Assert.That(currentCulture_Initial, Is.Null);
				Assert.That(currentUICulture_Initial, Is.Null);

				Assert.That(currentCulture_Final, Is.Null);
				Assert.That(currentUICulture_Final, Is.Null);

				Assert.That(currentCulture_Initial, Is.EqualTo(currentCulture_Final));
				Assert.That(currentUICulture_Initial, Is.EqualTo(currentUICulture_Final));

				Assert.That(currentUICulture_Initial, Is.EqualTo(currentCulture_Initial));
				Assert.That(currentUICulture_Final, Is.EqualTo(currentCulture_Final));
			});
		}
		finally
		{
			LanguageService.PreferredLanguageChanged -= HandlePreferredLanguageChanged;
		}

		void HandlePreferredLanguageChanged(object? sender, string? e) => didPreferredLanguageChangeFire = true;
	}

	[Test]
	public async Task ConfirmLanguageChanges_SetPreferedLanguage()
	{
		//Arrange
		CultureInfo? currentCulture_Initial, currentUICulture_Initial, currentCulture_Final, currentUICulture_Final;

		//Don't execute null or string.Empty first to ensure PreferredLanguageChanged fires (default value of LanguageService.PerferredLanguage is null)
		var availableLanguages = new List<string?>();
		availableLanguages.AddRange(CultureConstants.CulturePickerOptions.Keys.Reverse());
		availableLanguages.Insert(1, null);

		var languageService = ServiceCollection.ServiceProvider.GetRequiredService<LanguageService>();

		foreach (var language in availableLanguages)
		{
			bool didPreferredLanguageChangeFire = false;
			var preferredLanguageChangedTCS = new TaskCompletionSource<string?>();

			LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

			//Act
			currentCulture_Initial = CultureInfo.DefaultThreadCurrentCulture;
			currentUICulture_Initial = CultureInfo.DefaultThreadCurrentUICulture;

			languageService.PreferredLanguage = language;
			var preferredLanguageChangedResult = await preferredLanguageChangedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

			currentCulture_Final = CultureInfo.DefaultThreadCurrentCulture;
			currentUICulture_Final = CultureInfo.DefaultThreadCurrentUICulture;

			//Assert
			Assert.That(didPreferredLanguageChangeFire, Is.True);

			if (string.IsNullOrWhiteSpace(language))
			{
				Assert.Multiple(() =>
				{
					Assert.That(currentCulture_Final, Is.Null);
					Assert.That(currentUICulture_Final, Is.Null);
				});
			}
			else
			{
				Assert.Multiple(() =>
				{
					Assert.That(currentCulture_Final, Is.Not.Null);
					Assert.That(currentUICulture_Final, Is.Not.Null);
				});
			}

			Assert.Multiple(() =>
			{
				Assert.That(currentCulture_Initial, Is.Not.EqualTo(currentCulture_Final));
				Assert.That(currentUICulture_Initial, Is.Not.EqualTo(currentUICulture_Final));

				Assert.That(currentUICulture_Initial, Is.EqualTo(currentCulture_Initial));
				Assert.That(currentUICulture_Final, Is.EqualTo(currentCulture_Final));

				Assert.That(currentCulture_Final?.Name, Is.EqualTo(string.IsNullOrWhiteSpace(language) ? null : language));
				Assert.That(currentUICulture_Final?.Name, Is.EqualTo(string.IsNullOrWhiteSpace(language) ? null : language));

				Assert.That(string.IsNullOrWhiteSpace(language) ? null : language, Is.EqualTo(preferredLanguageChangedResult));
				Assert.That(currentCulture_Final?.Name, Is.EqualTo(preferredLanguageChangedResult));
				Assert.That(currentUICulture_Final?.Name, Is.EqualTo(preferredLanguageChangedResult));
			});

			void HandlePreferredLanguageChanged(object? sender, string? e)
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
	public void ConfirmError_SetPreferredLanguage(string language)
	{
		//Ensure test case is not supported by GitTrends
		Assert.That(CultureConstants.CulturePickerOptions.Keys, Does.Not.Contain(language));

		//Arrange
		var availableLanguages = CultureConstants.CulturePickerOptions.Keys;
		var languageService = ServiceCollection.ServiceProvider.GetRequiredService<LanguageService>();

		bool didPreferredLanguageChangeFire = false;
		LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

		//Act
		try
		{
			Assert.Throws<ArgumentException>(() => languageService.PreferredLanguage = language);

			//Assert
			Assert.That(didPreferredLanguageChangeFire, Is.False);
		}
		finally
		{
			LanguageService.PreferredLanguageChanged -= HandlePreferredLanguageChanged;
		}

		void HandlePreferredLanguageChanged(object? sender, string? e) => didPreferredLanguageChangeFire = true;
	}
}