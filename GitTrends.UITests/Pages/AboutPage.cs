using System.Collections.Generic;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
	class AboutPage : BasePage
	{
		readonly Query _requestFeatureButton, _viewOnGitHubButton, _watchersLabel, _starsLabel, _forksLabel;

		public AboutPage(IApp app) : base(app, () => PageTitles.AboutPage)
		{
			_forksLabel = GenerateMarkedQuery(AboutPageAutomationIds.ForksLabel);
			_starsLabel = GenerateMarkedQuery(AboutPageAutomationIds.StarsLabel);
			_watchersLabel = GenerateMarkedQuery(AboutPageAutomationIds.WatchersLabel);
			_viewOnGitHubButton = GenerateMarkedQuery(AboutPageAutomationIds.ViewOnGitHubButton);
			_requestFeatureButton = GenerateMarkedQuery(AboutPageAutomationIds.RequestFeatureButton);
		}

		public int? ForkCount => string.IsNullOrWhiteSpace(GetText(_forksLabel)) ? null : int.Parse(GetText(_forksLabel));
		public int? StarsCount => string.IsNullOrWhiteSpace(GetText(_starsLabel)) ? null : int.Parse(GetText(_starsLabel));
		public int? WatchersCount => string.IsNullOrWhiteSpace(GetText(_watchersLabel)) ? null : int.Parse(GetText(_watchersLabel));

		public IReadOnlyList<Contributor> Contributors => App.InvokeBackdoorMethod<IReadOnlyList<Contributor>>(BackdoorMethodConstants.GetContributorsCollection);
		public IReadOnlyList<NuGetPackageModel> InstalledLibraries => App.InvokeBackdoorMethod<IReadOnlyList<NuGetPackageModel>>(BackdoorMethodConstants.GetLibrariesCollection);

		public void TapViewOnGitHubButton()
		{
			App.Tap(_viewOnGitHubButton);
			App.Screenshot("View on GitHub Button Tapped");
		}

		public void TapRequestFeatureButton()
		{
			App.Tap(_requestFeatureButton);
			App.Screenshot("Request Feature Button Tapped");
		}
	}
}