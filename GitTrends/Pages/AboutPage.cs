using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;
using static GitTrends.MauiService;

namespace GitTrends;

class AboutPage : BaseContentPage<AboutViewModel>
{
	readonly DeepLinkingService _deepLinkingService;

	public AboutPage(AboutViewModel aboutViewModel,
		IDeviceInfo deviceInfo,
		IAnalyticsService analyticsService,
		DeepLinkingService deepLinkingService) : base(aboutViewModel, analyticsService)
	{
		const int horizontalPadding = 28;

		var titleRowHeight = IsSmallScreen ? 16 : 20;
		var descriptionRowHeight = IsSmallScreen ? 28 : 32;

		_deepLinkingService = deepLinkingService;

		Title = PageTitles.AboutPage;

		Content = new ScrollView
		{
			Content = new Grid
			{
				ColumnSpacing = 2,
				RowSpacing = 6,

				ColumnDefinitions = Columns.Define(
					(Column.LeftPadding, horizontalPadding),
					(Column.Icon, 108),
					(Column.Statistics, Star),
					(Column.RightPadding, horizontalPadding)),

				RowDefinitions = Rows.Define(
					(Row.Title, IsSmallScreen ? 28 : 32),
					(Row.Description, IsSmallScreen ? 44 : 48),
					(Row.Statistics, IsSmallScreen ? 28 : 36),
					(Row.ActionButtons, IsSmallScreen ? 64 : 68),
					(Row.CollaboratorTitle, titleRowHeight),
					(Row.CollaboratorDescription, descriptionRowHeight),
					(Row.CollaboratorCollection, IsSmallScreen ? 84 : 100),
					(Row.LibrariesTitle, titleRowHeight),
					(Row.LibrariesDescription, descriptionRowHeight),
					(Row.LibrariesCollection, Star)),

				Children =
				{
					new Image().Margins(right: IsSmallScreen ? 8 : 12)
						.Row(Row.Title).RowSpan(3).Column(Column.Icon)
						.DynamicResource(Image.SourceProperty, nameof(BaseTheme.GitTrendsImageSource)),

					new Label { Text = GitHubConstants.GitTrendsRepoName }.Font(FontFamilyConstants.RobotoMedium, 24)
						.Row(Row.Title).Column(Column.Statistics)
						.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor)),

					new DescriptionLabel(AboutPageConstants.DescriptionText) { MaxLines = 3 }
						.Row(Row.Description).Column(Column.Statistics),

					new GitTrendsStatisticsView(deviceInfo)
						.Row(Row.Statistics).Column(Column.Statistics),

					new ButtonLayout(deviceInfo).Center()
						.Row(Row.ActionButtons).Column(Column.Icon).ColumnSpan(2),

					new TitleLabel(AboutPageConstants.CollaboratorsTitle).TextBottom()
						.Row(Row.CollaboratorTitle).Column(Column.Icon).ColumnSpan(2),

					new DescriptionLabel(AboutPageConstants.CollaboratorsDescription)
						.Row(Row.CollaboratorDescription).Column(Column.Icon).ColumnSpan(2),

					new CollectionView
						{
							HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
							Header = new BoxView().Width(horizontalPadding),
							SelectionMode = SelectionMode.Single,
							ItemTemplate = new ContributorDataTemplate(),
							ItemsSource = BindingContext.GitTrendsContributors,
							ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
						}.Top().Margins(top: 4).Height(ContributorDataTemplate.RowHeight + 8)
						.Row(Row.CollaboratorCollection).ColumnSpan(All<Column>())
						.Invoke(collectionView => collectionView.SelectionChanged += HandleContributorSelectionChanged),

					new TitleLabel(AboutPageConstants.LibrariesTitle)
						.Row(Row.LibrariesTitle).Column(Column.Icon).ColumnSpan(2),

					new DescriptionLabel(AboutPageConstants.LibrariesDescription)
						.Row(Row.LibrariesDescription).Column(Column.Icon).ColumnSpan(2),

					new CollectionView
						{
							HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
							Header = new BoxView { WidthRequest = horizontalPadding },
							SelectionMode = SelectionMode.Single,
							ItemTemplate = new LibraryDataTemplate(),
							ItemsSource = BindingContext.InstalledLibraries,
							ItemsLayout = IsSmallScreen ? new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) : new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal)
						}.Top().Height(IsSmallScreen ? LibraryDataTemplate.RowHeight + 8 : LibraryDataTemplate.RowHeight * 2 + 8)
						.Row(Row.LibrariesCollection).ColumnSpan(All<Column>())
						.Invoke(collectionView => collectionView.SelectionChanged += HandleLibrarySelectionChanged),
				}
			}
		}.Paddings(0, 16, 0, 0);
	}

	enum Row { Title, Description, Statistics, ActionButtons, CollaboratorTitle, CollaboratorDescription, CollaboratorCollection, LibrariesTitle, LibrariesDescription, LibrariesCollection }
	enum Column { LeftPadding, Icon, Statistics, RightPadding }

	async void HandleContributorSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		var collectionView = (CollectionView)sender;
		collectionView.SelectedItem = null;

		if (e.CurrentSelection.FirstOrDefault() is Contributor contributor)
		{
			AnalyticsService.Track("Contributor Tapped", nameof(Contributor.Login), contributor.Login);

			await _deepLinkingService.OpenBrowser(contributor.GitHubUrl, CancellationToken.None);
		}
	}

	async void HandleLibrarySelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		var collectionView = (CollectionView)sender;
		collectionView.SelectedItem = null;

		if (e.CurrentSelection.FirstOrDefault() is NuGetPackageModel nuGetPackageModel)
		{
			AnalyticsService.Track("Library Tapped", nameof(NuGetPackageModel.PackageName), nuGetPackageModel.PackageName);

			await _deepLinkingService.OpenBrowser(nuGetPackageModel.WebsiteUri, CancellationToken.None);
		}
	}

	sealed class TitleLabel : Label
	{
		public TitleLabel(in string text)
		{
			Text = text;
			LineBreakMode = LineBreakMode.TailTruncation;

			this.Font(FontFamilyConstants.RobotoMedium, IsSmallScreen ? 14 : 16).Start().DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
		}
	}

	sealed class DescriptionLabel : Label
	{
		public DescriptionLabel(in string text)
		{
			Text = text;
			MaxLines = 2;
			LineBreakMode = LineBreakMode.TailTruncation;

			this.Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12).Start().DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
		}
	}

	sealed class ButtonLayout : HorizontalStackLayout
	{
		public ButtonLayout(IDeviceInfo deviceInfo)
		{
			Spacing = 16;

			Margin = new Thickness(0, 16);

			Children.Add(new ViewOnGitHubButton(deviceInfo).End());
			Children.Add(new RequestFeatureButton(deviceInfo).Start());
		}

		sealed class ViewOnGitHubButton(IDeviceInfo deviceInfo)
			: AboutPageButton("github.svg", AboutPageConstants.GitHubButton, AboutPageAutomationIds.ViewOnGitHubButton, Color.FromArgb("231F20"), nameof(AboutViewModel.ViewOnGitHubCommand), deviceInfo)
		{
		}

		sealed class RequestFeatureButton(IDeviceInfo deviceInfo)
			: AboutPageButton("sparkle.svg", AboutPageConstants.RequestFeatureButton, AboutPageAutomationIds.RequestFeatureButton, Color.FromArgb("F97B4F"), nameof(AboutViewModel.RequestFeatureCommand), deviceInfo)
		{
		}


		abstract class AboutPageButton : SvgTextLabel
		{
			protected AboutPageButton(in string svgFileName, in string text, in string automationId, in Color backgroundColor, in string commandPropertyBindingPath, in IDeviceInfo deviceInfo)
				: base(deviceInfo, svgFileName, text, automationId, IsSmallScreen ? 12 : 14, FontFamilyConstants.RobotoMedium, 4)
			{
				BackgroundColor = backgroundColor;
				Padding = IsSmallScreen ? new Thickness(deviceInfo.Platform == DevicePlatform.iOS ? 8 : 12, 8) : new Thickness(deviceInfo.Platform == DevicePlatform.iOS ? 10 : 16, 8);
				GestureRecognizers.Add(new TapGestureRecognizer().Bind(TapGestureRecognizer.CommandProperty, commandPropertyBindingPath));
			}
		}
	}
}