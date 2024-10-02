using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends.UnitTests;

class ResxTests : BaseTest
{
	[TestCase(typeof(AboutPageConstants))]
	[TestCase(typeof(AppStoreRatingRequestConstants))]
	[TestCase(typeof(DemoUserConstants))]
	[TestCase(typeof(EmptyDataViewConstantsInternal))]
	[TestCase(typeof(GitHubLoginButtonConstants))]
	[TestCase(typeof(ManageOrganizationsConstants))]
	[TestCase(typeof(NotificationConstants))]
	[TestCase(typeof(OnboardingConstants))]
	[TestCase(typeof(PageTitles))]
	[TestCase(typeof(PullToRefreshFailedConstants))]
	[TestCase(typeof(ReferringSitesPageConstants))]
	[TestCase(typeof(RepositoryPageConstants))]
	[TestCase(typeof(ReviewServiceConstants))]
	[TestCase(typeof(SettingsPageConstants))]
	[TestCase(typeof(SortingConstants))]
	[TestCase(typeof(SplashScreenPageConstants))]
	[TestCase(typeof(ThemeConstants))]
	[TestCase(typeof(TrendsChartTitleConstants))]
	[TestCase(typeof(WelcomePageConstants))]
	public void ConfirmCulturesExists(Type resxType)
	{
		//Arrange
		var cultureNames = CultureConstants.CulturePickerOptions.Keys;
		var resxCultureInfoList = GetAvailableResxCultureInfos(resxType.Assembly);

		//Act
		foreach (var cultureInfo in resxCultureInfoList)
		{
			foreach (var cultureName in cultureNames)
			{
				//Assert
				Assert.That(resxCultureInfoList.Any(x => x.Name == cultureName));
			}
		}
	}

	[TestCase(typeof(AboutPageConstants))]
	[TestCase(typeof(AppStoreRatingRequestConstants))]
	[TestCase(typeof(DemoUserConstants))]
	[TestCase(typeof(EmptyDataViewConstantsInternal))]
	[TestCase(typeof(GitHubLoginButtonConstants))]
	[TestCase(typeof(ManageOrganizationsConstants))]
	[TestCase(typeof(NotificationConstants))]
	[TestCase(typeof(OnboardingConstants))]
	[TestCase(typeof(PageTitles))]
	[TestCase(typeof(PullToRefreshFailedConstants))]
	[TestCase(typeof(ReferringSitesPageConstants))]
	[TestCase(typeof(RepositoryPageConstants))]
	[TestCase(typeof(ReviewServiceConstants))]
	[TestCase(typeof(SettingsPageConstants))]
	[TestCase(typeof(SortingConstants))]
	[TestCase(typeof(SplashScreenPageConstants))]
	[TestCase(typeof(ThemeConstants))]
	[TestCase(typeof(TrendsChartTitleConstants))]
	[TestCase(typeof(WelcomePageConstants))]
	public void ConfirmTranslationsAreComplete(Type resxType)
	{
		//Arrange
		var filesWithExtraEntries = new List<ResxFile>();
		var filesWithMissingEntryData = new List<ResxFile>();
		var filesWithMissingEntryValue = new List<ResxFile>();

		var resxFiles = GetResxFiles(resxType);

		//Act
		var defaultResx = GetDefaultResx(resxFiles);

		foreach (var fileWithExtraEntries in GetExtraEntries(resxFiles, defaultResx))
		{
			if (filesWithExtraEntries.Any(x => x.Language == fileWithExtraEntries.Language))
				filesWithExtraEntries.First(x => x.Language == fileWithExtraEntries.Language).Entries.AddRange(fileWithExtraEntries.Entries);
			else
				filesWithExtraEntries.Add(new ResxFile(fileWithExtraEntries.Language, fileWithExtraEntries.Entries));
		}

		foreach (var fileWithMissingEntryData in GetFilesWithMissingEntryData(resxFiles, defaultResx))
		{
			if (filesWithMissingEntryData.Any(x => x.Language == fileWithMissingEntryData.Language))
				filesWithMissingEntryData.First(x => x.Language == fileWithMissingEntryData.Language).Entries.AddRange(fileWithMissingEntryData.Entries);
			else
				filesWithMissingEntryData.Add(new ResxFile(fileWithMissingEntryData.Language, fileWithMissingEntryData.Entries));
		}

		foreach (var fileWithMissingEntryValues in GetFilesWithMissingEntryValue(resxFiles))
		{
			if (filesWithMissingEntryValue.Any(x => x.Language == fileWithMissingEntryValues.Language))
				filesWithMissingEntryValue.First(x => x.Language == fileWithMissingEntryValues.Language).Entries.AddRange(fileWithMissingEntryValues.Entries);
			else
				filesWithMissingEntryValue.Add(new ResxFile(fileWithMissingEntryValues.Language, fileWithMissingEntryValues.Entries));
		}

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(filesWithExtraEntries, Is.Empty); // Extra Translations Found
			Assert.That(filesWithMissingEntryData, Is.Empty); // Missing Data Found
			Assert.That(filesWithMissingEntryValue, Is.Empty); // Missing Values Found
		});
	}

	static List<ResxEntryModel> GetDefaultResx(in List<ResxFile> resxFiles) => resxFiles.First(static x => x.Language is "").Entries;

	//https://stackoverflow.com/a/41760659/5953643
	static CultureInfo[] GetAvailableResxCultureInfos(Assembly assembly)
	{
		// must have invariant culture
		var assemblyResxCultures = new HashSet<CultureInfo>
		{
			CultureInfo.InvariantCulture
		};

		var names = assembly.GetManifestResourceNames();

		if (names.Length > 0)
		{
			var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

			const string resourcesEnding = ".resources";

			for (int i = 0; i < names.Length; i++)
			{
				var name = names[i];

				if (string.IsNullOrWhiteSpace(name)
					|| name.Length <= resourcesEnding.Length
					|| !name.EndsWith(resourcesEnding, StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}

				name = name.Remove(name.Length - resourcesEnding.Length, resourcesEnding.Length);

				if (string.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				var resourceManager = new ResourceManager(name, assembly);

				for (int j = 0; j < allCultures.Length; j++)
				{
					var culture = allCultures[j];

					try
					{
						// we got InvariantCulture
						// don't use "==", it does not work
						if (culture.Equals(CultureInfo.InvariantCulture))
						{
							continue;
						}

						using var resourceSet = resourceManager.GetResourceSet(culture, true, false);
						if (resourceSet is not null)
						{
							assemblyResxCultures.Add(culture);
						}
					}
					catch (CultureNotFoundException)
					{
						// NOP
					}
				}
			}
		}

		return [.. assemblyResxCultures];
	}

	//https://stackoverflow.com/a/41760659/5953643
	static List<ResxFile> GetResxFiles(Type type)
	{
		var assertionExceptionList = new List<AssertionException>();

		if (type?.FullName is null)
			throw new ArgumentException($"{nameof(Type.FullName)} cannot be null");

		var availableResxsCultureInfos = GetAvailableResxCultureInfos(type.Assembly);

		var resourceManager = new ResourceManager(type.FullName, type.Assembly);

		var resxFiles = new List<ResxFile>();

		for (int i = 0; i < availableResxsCultureInfos.Length; i++)
		{
			var cultureInfo = availableResxsCultureInfos[i];

			var resourceSet = resourceManager.GetResourceSet(cultureInfo, true, false);

			if (resourceSet is null)
			{
				assertionExceptionList.Add(new AssertionException($"The language \"{cultureInfo.Name}\" is not specified in \"{type}\"."));
				continue;
			}

			var resxEntryModelList = new List<ResxEntryModel>();

			foreach (DictionaryEntry? item in resourceSet)
			{
				if (item is null)
					continue;

				var key = item.Value.Key.ToString();
				var value = item.Value.Value;

				if (key is null || value is null)
					continue;

				resxEntryModelList.Add(new ResxEntryModel(type, key, value));
			}

			resxFiles.Add(new ResxFile(cultureInfo.Name, resxEntryModelList));
		}

		if (assertionExceptionList.Any())
			throw new AggregateException(assertionExceptionList);

		return resxFiles;
	}

	static List<ResxFile> GetExtraEntries(List<ResxFile> resxFiles, List<ResxEntryModel> neutralLanguage)
	{
		var extraEntryFileList = new List<ResxFile>();

		foreach (var resxFile in resxFiles)
		{
			var resxEntryModelList = resxFile.Entries;

			var extraEntryList = new List<ResxEntryModel>();

			foreach (var resxEntryModel in resxEntryModelList)
			{
				if (!neutralLanguage.Any(x => x.Data == resxEntryModel.Data))
				{
					extraEntryList.Add(resxEntryModel);
				}
			}

			if (extraEntryList.Count > 0)
			{
				extraEntryFileList.Add(new ResxFile(resxFile.Language, extraEntryList));
			}
		}
		return extraEntryFileList;
	}

	static List<ResxFile> GetFilesWithMissingEntryValue(in List<ResxFile> resxFiles)
	{
		var emptyResxFiles = new List<ResxFile>();

		foreach (var resxFile in resxFiles)
		{
			var resxEntryModelList = resxFile.Entries;

			var emptyResxList = new List<ResxEntryModel>();

			foreach (var resxEntryModel in resxEntryModelList)
			{
				if (resxEntryModel.Value is null)
				{
					emptyResxList.Add(resxEntryModel);
					continue;
				}

				var stringValue = resxEntryModel.Value as string;
				if (string.IsNullOrWhiteSpace(stringValue))
				{
					emptyResxList.Add(resxEntryModel);
				}
			}

			if (emptyResxList.Count > 0)
			{
				emptyResxFiles.Add(new ResxFile(resxFile.Language, emptyResxList));
			}
		}

		return emptyResxFiles;
	}

	static List<ResxFile> GetFilesWithMissingEntryData(List<ResxFile> resxFiles, List<ResxEntryModel> neutralLanguageResxEntryList)
	{
		var missingEntryFileList = new List<ResxFile>();

		foreach (var resxFile in resxFiles)
		{
			var resxEntryModelList = resxFile.Entries;

			var missingEntryList = new List<ResxEntryModel>();

			foreach (var naturalLanguageResxEntry in neutralLanguageResxEntryList)
			{
				if (!resxEntryModelList.Any(x => x.Data == naturalLanguageResxEntry.Data))
				{
					missingEntryList.Add(naturalLanguageResxEntry);
				}
			}

			if (missingEntryList.Count > 0)
			{
				missingEntryFileList.Add(new ResxFile(resxFile.Language, missingEntryList));
			}
		}

		return missingEntryFileList;
	}

	class ResxFile(in string language)
	{
		public ResxFile(in string language, in IEnumerable<ResxEntryModel> resxEntryModels)
			: this(language) => Entries.AddRange(resxEntryModels);

		public string Language { get; } = language;
		public List<ResxEntryModel> Entries { get; } = [];

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(Language);

			foreach (var entry in Entries)
			{
				stringBuilder.Append('\t');
				stringBuilder.AppendLine(entry.ToString());
			}

			return stringBuilder.ToString();
		}
	}

	class ResxEntryModel
	{
		public ResxEntryModel(in Type type, in string data, in object value) => (Type, Data, Value) = (type, data, value);

		public Type Type { get; }
		public string Data { get; }
		public object Value { get; }

		public override string ToString() => $"{Type.Name} [ {Data} : {Value} ]";
	}
}