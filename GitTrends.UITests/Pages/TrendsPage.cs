using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;

using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
	class TrendsPage : BaseCarouselPage
	{
		readonly Query _viewsClonesChart, _activityIndicator, _androidContextMenuOverflowButton, _referringSiteButton, _viewsCard,
			_uniqueViewsCard, _clonesCard, _uniqueClonesCard, _viewsStatisticsLabel, _uniqueViewsStatisticsLabel,
			_clonesStatisticsLabel, _uniqueClonesStatisticsLabel, _emptyViewsClonesDataView, _emptyStarsDataView,
			_starsChart, _starsStatisticsLabel, _starsHeaderMessageLabel;

		public TrendsPage(IApp app) : base(app, BackdoorMethodConstants.GetCurrentTrendsPageNumber)
		{
			_starsChart = GenerateMarkedQuery(TrendsPageAutomationIds.StarsChart);
			_viewsClonesChart = GenerateMarkedQuery(TrendsPageAutomationIds.ViewsClonesChart);

			_viewsCard = GenerateMarkedQuery(TrendsPageAutomationIds.ViewsCard);
			_uniqueViewsCard = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueViewsCard);
			_clonesCard = GenerateMarkedQuery(TrendsPageAutomationIds.ClonesCard);
			_uniqueClonesCard = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueClonesCard);

			_activityIndicator = GenerateMarkedQuery(TrendsPageAutomationIds.ActivityIndicator);

			_androidContextMenuOverflowButton = x => x.Class("androidx.appcompat.widget.ActionMenuPresenter$OverflowMenuButton");
			_referringSiteButton = GenerateMarkedQuery(TrendsPageAutomationIds.ReferringSitesButton);

			_viewsStatisticsLabel = GenerateMarkedQuery(TrendsPageAutomationIds.ViewsStatisticsLabel);
			_uniqueViewsStatisticsLabel = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueViewsStatisticsLabel);
			_clonesStatisticsLabel = GenerateMarkedQuery(TrendsPageAutomationIds.ClonesStatisticsLabel);
			_uniqueClonesStatisticsLabel = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueClonesStatisticsLabel);

			_starsStatisticsLabel = GenerateMarkedQuery(TrendsPageAutomationIds.StarsStatisticsLabel);
			_starsHeaderMessageLabel = GenerateMarkedQuery(TrendsPageAutomationIds.StarsHeaderMessageLabel);

			_emptyStarsDataView = GenerateMarkedQuery(TrendsPageAutomationIds.StarsEmptyDataView);
			_emptyViewsClonesDataView = GenerateMarkedQuery(TrendsPageAutomationIds.ViewsClonesEmptyDataView);
		}

		public string ViewsStatisticsLabelText => GetText(_viewsStatisticsLabel);
		public string ClonesStatisticsLabelText => GetText(_clonesStatisticsLabel);
		public string StarsHeaderMessageLabelText => GetText(_starsHeaderMessageLabel);
		public string UniqueViewsStatisticsLabelText => GetText(_uniqueViewsStatisticsLabel);
		public string UniqueClonesStatisticsLabelText => GetText(_uniqueClonesStatisticsLabel);

		public string StarsStatisticsLabelText => GetText(_starsStatisticsLabel);

		public bool IsEmptyStarsDataViewVisible => App.Query(_emptyStarsDataView).Any();
		public bool IsEmptyViewsClonesDataViewVisible => App.Query(_emptyViewsClonesDataView).Any();

		public bool IsStarsChartVisible => App.Query(_starsChart).Any();
		public bool IsViewsClonesChartVisible => App.Query(_viewsClonesChart).Any();

		public override Task WaitForPageToLoad(TimeSpan? timespan = null)
		{
			try
			{
				App.WaitForElement(_activityIndicator, timeout: TimeSpan.FromSeconds(2));
			}
			catch
			{

			}

			App.WaitForNoElement(_activityIndicator, timeout: timespan);

			TryDismissSyncfusionLicensePopup();

			switch (CurrentPageNumber)
			{
				case 0:
					App.WaitForElement(_viewsClonesChart, timeout: timespan);
					break;
				case 1:
					App.WaitForElement(_starsChart, timeout: timespan);
					break;

				default:
					throw new NotImplementedException();
			}

			return Task.CompletedTask;
		}

		public void WaitForEmptyViewsClonesDataView()
		{
			App.WaitForElement(_emptyViewsClonesDataView);
			App.Screenshot("Empty Views Clones Data View Appeared");
		}

		public void WaitForEmptyStarsDataView()
		{
			App.WaitForElement(_emptyStarsDataView);
			App.Screenshot("Empty Stars Data View Appeared");
		}

		public bool IsViewsClonesChartSeriesVisible(string seriesName)
		{
			var serializedIsSeriesVisible = App.InvokeBackdoorMethod(BackdoorMethodConstants.IsViewsClonesChartSeriesVisible, seriesName).ToString();
			return JsonConvert.DeserializeObject<bool>(serializedIsSeriesVisible);
		}

		public bool IsStarsChartSeriesVisible()
		{
			var serializedIsSeriesVisible = App.InvokeBackdoorMethod(BackdoorMethodConstants.IsViewsClonesChartSeriesVisible, TrendsChartTitleConstants.StarsTitle).ToString();
			return JsonConvert.DeserializeObject<bool>(serializedIsSeriesVisible);
		}

		public void TapBackButton()
		{
			App.Back();
			App.Screenshot("Back Button Tapped");
		}

		public void TapViewsCard()
		{
			App.Tap(_viewsCard);
			App.Screenshot("Views Card Tapped");
		}

		public void TapUniqueViewsCard()
		{
			App.Tap(_uniqueViewsCard);
			App.Screenshot("Unique Views Card Tapped");
		}

		public void TapClonesCard()
		{
			App.Tap(_clonesCard);
			App.Screenshot("Clones Card Tapped");
		}

		public void TapUniqueClonesCard()
		{
			App.Tap(_uniqueClonesCard);
			App.Screenshot("Unique Clones Card Tapped");
		}

		public void TapReferringSitesButton()
		{
			App.Tap(_referringSiteButton);
			App.Screenshot("Referring Sites Button Tapped");
		}
	}
}