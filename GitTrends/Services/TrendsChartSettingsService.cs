using System;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class TrendsChartSettingsService
    {
        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;

        public TrendsChartSettingsService(IAnalyticsService analyticsService, IPreferences preferences) =>
            (_analyticsService, _preferences) = (analyticsService, preferences);

        public TrendsChartOption CurrentTrendsChartOption
        {
            get => GetCurrentTrendsChartOption();
            set => SetCurrentTrendsChartOption(value);
        }

        public bool ShouldShowClonesByDefault
        {
            get => _preferences.Get(nameof(ShouldShowClonesByDefault), true);
            set => _preferences.Set(nameof(ShouldShowClonesByDefault), value);
        }

        public bool ShouldShowUniqueClonesByDefault
        {
            get => _preferences.Get(nameof(ShouldShowUniqueClonesByDefault), true);
            set => _preferences.Set(nameof(ShouldShowUniqueClonesByDefault), value);
        }

        public bool ShouldShowViewsByDefault
        {
            get => _preferences.Get(nameof(ShouldShowViewsByDefault), true);
            set => _preferences.Set(nameof(ShouldShowViewsByDefault), value);
        }

        public bool ShouldShowUniqueViewsByDefault
        {
            get => _preferences.Get(nameof(ShouldShowUniqueViewsByDefault), true);
            set => _preferences.Set(nameof(ShouldShowUniqueViewsByDefault), value);
        }

        TrendsChartOption GetCurrentTrendsChartOption()
        {
            if (!ShouldShowUniqueClonesByDefault
                && !ShouldShowUniqueViewsByDefault
                && ShouldShowClonesByDefault
                && ShouldShowViewsByDefault)
            {
                return TrendsChartOption.NoUniques;
            }

            if (ShouldShowUniqueClonesByDefault
                && ShouldShowUniqueViewsByDefault
                && !ShouldShowClonesByDefault
                && !ShouldShowViewsByDefault)
            {
                return TrendsChartOption.JustUniques;
            }

            //All is the Default Value 
            ShouldShowUniqueClonesByDefault = true;
            ShouldShowUniqueViewsByDefault = true;
            ShouldShowClonesByDefault = true;
            ShouldShowViewsByDefault = true;

            return TrendsChartOption.All;
        }

        void SetCurrentTrendsChartOption(in TrendsChartOption currentTrendsChartOption)
        {
            _analyticsService.Track($"{nameof(TrendsChartOption)} changed", nameof(TrendsChartOption), currentTrendsChartOption.ToString());

            switch (currentTrendsChartOption)
            {
                case TrendsChartOption.All:
                    ShouldShowUniqueClonesByDefault = true;
                    ShouldShowUniqueViewsByDefault = true;
                    ShouldShowClonesByDefault = true;
                    ShouldShowViewsByDefault = true;
                    break;

                case TrendsChartOption.JustUniques:
                    ShouldShowUniqueClonesByDefault = true;
                    ShouldShowUniqueViewsByDefault = true;
                    ShouldShowClonesByDefault = false;
                    ShouldShowViewsByDefault = false;
                    break;

                case TrendsChartOption.NoUniques:
                    ShouldShowUniqueClonesByDefault = false;
                    ShouldShowUniqueViewsByDefault = false;
                    ShouldShowClonesByDefault = true;
                    ShouldShowViewsByDefault = true;
                    break;

                default:
                    throw new NotSupportedException($"{currentTrendsChartOption} not supported");
            }
        }
    }
}
