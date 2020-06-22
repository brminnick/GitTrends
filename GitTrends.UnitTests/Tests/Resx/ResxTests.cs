using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class ResxTests : BaseTest
    {
        static readonly List<Type> _resxTypeList = new List<Type>
        {
            typeof(AppStoreRatingRequestConstants),
            typeof(DemoUserConstants),
            typeof(EmptyDataViewConstants),
            typeof(GitHubLoginButtonConstants),
            typeof(NotificationConstants),
            typeof(OnboardingConstants),
            typeof(PageTitles),
            typeof(ReferringSitesPageConstants),
            typeof(RepositoryPageConstants),
            typeof(ReviewServiceConstants),
            typeof(SettingsPageConstants),
            typeof(SortingConstants),
            typeof(SplashScreenPageConstants),
            typeof(TrendsChartTitleConstants),
            typeof(WelcomePageConstants)
        };

        [Test]
        public void ConfirmCulturesExists()
        {
            //Arrange
            var cultures = CultureConstants.CulturePickerOptions.Keys;
            var resxCultureInfoList = new List<CultureInfo[]>(_resxTypeList.Select(x => GetAvailableResxCultureInfos(x.Assembly)));

            //Act
            foreach (var cultureInfo in resxCultureInfoList)
            {
                foreach (var culture in cultures)
                {
                    //Assert
                    Assert.IsTrue(cultureInfo.Any(x => x.Name == culture));
                }
            }
        }

        [Test]
        public void ConfirmTranslationsAreComplete()
        {
            //Arrange
            var resxDictionaries = new List<Dictionary<string, Dictionary<string, object>>>(_resxTypeList.Select(x => GetResxDictionaries(x)));

            //Act
            foreach (var resxDictionary in resxDictionaries)
            {
                var defaultResx = GetDefaultResx(resxDictionary);

                //Assert
                Assert.IsEmpty(GetMissingEntries(resxDictionary, defaultResx));
                Assert.IsEmpty(GetDispensable(resxDictionary, defaultResx));
                Assert.IsEmpty(GetEmpty(resxDictionary));
            }
        }

        //https://stackoverflow.com/a/41760659/5953643
        static Dictionary<string, object> GetDefaultResx(Dictionary<string, Dictionary<string, object>> resxFiles)
        {
            if (!resxFiles.TryGetValue(string.Empty, out var neutralLanguage))
                throw new Exception(string.Format("The neutral language is not specified"));

            resxFiles.Remove(string.Empty);
            return neutralLanguage;
        }

        //https://stackoverflow.com/a/41760659/5953643
        static CultureInfo[] GetAvailableResxCultureInfos(Assembly assembly)
        {
            var assemblyResxCultures = new HashSet<CultureInfo>();

            // must have invariant culture
            assemblyResxCultures.Add(CultureInfo.InvariantCulture);

            string[] names = assembly.GetManifestResourceNames();

            if (names != null && names.Length > 0)
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
                            if (resourceSet != null)
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

            return assemblyResxCultures.ToArray();
        }

        //https://stackoverflow.com/a/41760659/5953643
        static Dictionary<string, Dictionary<string, object>> GetResxDictionaries(Type type)
        {
            if (type?.FullName is null)
                throw new ArgumentException($"{nameof(Type.FullName)} cannot be null");

            var availableResxsCultureInfos = GetAvailableResxCultureInfos(type.Assembly);

            var resourceManager = new ResourceManager(type.FullName, type.Assembly);

            var resxDictionaries = new Dictionary<string, Dictionary<string, object>>();

            for (int i = 0; i < availableResxsCultureInfos.Length; i++)
            {
                var cultureInfo = availableResxsCultureInfos[i];

                var resourceSet = resourceManager.GetResourceSet(cultureInfo, true, false) ?? throw new MultipleAssertException($"The language \"{cultureInfo.Name}\" is not specified in \"{type}\".");

                var dict = new Dictionary<string, object>();

                foreach (DictionaryEntry? item in resourceSet)
                {
                    if (item is null)
                        continue;

                    var key = item.Value.Key.ToString();
                    var value = item.Value.Value;

                    if (key is null || value is null)
                        continue;

                    dict.Add(key, value);
                }

                resxDictionaries.Add(cultureInfo.Name, dict);
            }

            return resxDictionaries;
        }

        //https://stackoverflow.com/a/41760659/5953643
        static Dictionary<string, List<string>> GetDispensable(Dictionary<string, Dictionary<string, object>> resxDictionaries, Dictionary<string, object> neutralLanguage)
        {
            var dispensable = new Dictionary<string, List<string>>();

            foreach (var pair in resxDictionaries)
            {
                var resxs = pair.Value;

                var list = new List<string>();

                foreach (var key in resxs.Keys)
                {
                    if (!neutralLanguage.ContainsKey(key))
                    {
                        list.Add(key);
                    }
                }

                if (list.Count > 0)
                {
                    dispensable.Add(pair.Key, list);
                }
            }
            return dispensable;
        }

        static Dictionary<string, List<string>> GetEmpty(Dictionary<string, Dictionary<string, object>> resxDictionaries)
        {
            var empty = new Dictionary<string, List<string>>();

            foreach (var pair in resxDictionaries)
            {
                var resxs = pair.Value;

                var list = new List<string>();

                foreach (var entrie in resxs)
                {
                    if (entrie.Value is null)
                    {
                        list.Add(entrie.Key);
                        continue;
                    }

                    var stringValue = entrie.Value as string;
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        list.Add(entrie.Key);
                    }
                }

                if (list.Count > 0)
                {
                    empty.Add(pair.Key, list);
                }
            }
            return empty;
        }

        static Dictionary<string, List<string>> GetMissingEntries(Dictionary<string, Dictionary<string, object>> resxDictionaries, Dictionary<string, object> neutralLanguage)
        {
            var missing = new Dictionary<string, List<string>>();

            foreach (var pair in resxDictionaries)
            {
                var resxs = pair.Value;

                var list = new List<string>();

                foreach (var key in neutralLanguage.Keys)
                {
                    if (!resxs.ContainsKey(key))
                    {
                        list.Add(key);
                    }
                }

                if (list.Count > 0)
                {
                    missing.Add(pair.Key, list);
                }
            }

            return missing;
        }
    }
}