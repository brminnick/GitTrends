using System;
using GitTrends.Mobile.Shared;
using Xamarin.Essentials;

namespace GitTrends
{
    public class TrendsChartSettingsService
    {
        readonly AnalyticsService _analyticsService;

        public TrendsChartSettingsService(AnalyticsService analyticsService) => _analyticsService = analyticsService;

        public TrendsChartOption CurrentTrendsChartOption
        {
            get => GetCurrentTrendsChartOption();
            set => SetCurrentTrendsChartOption(value);
        }

        public bool ShouldShowClonesByDefault
        {
            get => Preferences.Get(nameof(ShouldShowClonesByDefault), true);
            set => Preferences.Set(nameof(ShouldShowClonesByDefault), value);
        }

        public bool ShouldShowUniqueClonesByDefault
        {
            get => Preferences.Get(nameof(ShouldShowUniqueClonesByDefault), true);
            set => Preferences.Set(nameof(ShouldShowUniqueClonesByDefault), value);
        }

        public bool ShouldShowViewsByDefault
        {
            get => Preferences.Get(nameof(ShouldShowViewsByDefault), true);
            set => Preferences.Set(nameof(ShouldShowViewsByDefault), value);
        }

        public bool ShouldShowUniqueViewsByDefault
        {
            get => Preferences.Get(nameof(ShouldShowUniqueViewsByDefault), true);
            set => Preferences.Set(nameof(ShouldShowUniqueViewsByDefault), value);
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
