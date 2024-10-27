using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

class GitHubUserView : Grid
{
	public const int TotalHeight = _imageHeight + _nameLabelHeight + _aliasLabelHeight + _topMargin + _bottomMargin + _rowSpacing * 3;

	const int _imageHeight = 140;
	const int _topMargin = 16;
	const int _bottomMargin = 8;
	const int _widthMargin = 16;
	const int _nameLabelHeight = 24;
	const int _aliasLabelHeight = 18;
	const int _rowSpacing = 4;

	public GitHubUserView()
	{
		AutomationId = SettingsPageAutomationIds.GitHubUserView;

		Margin = new Thickness(_widthMargin, _topMargin, _widthMargin, _bottomMargin);
		RowSpacing = 4;

		RowDefinitions = Rows.Define(
			(Row.Image, _imageHeight),
			(Row.Name, _nameLabelHeight),
			(Row.Alias, _aliasLabelHeight));

		Children.Add(new GitHubAvatarImage().Row(Row.Image));
		Children.Add(new NameLabel().Row(Row.Name));
		Children.Add(new IsAuthenticatingActivityIndicator().Row(Row.Name).RowSpan(2));
		Children.Add(new AliasLabel().Row(Row.Alias));
		Children.Add(new TryDemoButton().Row(Row.Alias));

		this.BindTapGesture(nameof(SettingsViewModel.GitHubUserViewTappedCommand));
	}

	enum Row { Image, Name, Alias }

	sealed class GitHubAvatarImage : CircleImage
	{
		public GitHubAvatarImage()
		{
			this.Center();

			HeightRequest = WidthRequest = _imageHeight;

			AutomationId = SettingsPageAutomationIds.GitHubAvatarImage;

			Stroke = Colors.Transparent;

			this.Bind(ImageSourceProperty, nameof(SettingsViewModel.GitHubAvatarImageSource))
				.DynamicResources(
					(ErrorPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource)),
					(LoadingPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource)));
		}
	}

	sealed class NameLabel : Label
	{
		public NameLabel()
		{
			this.Center();

			AutomationId = SettingsPageAutomationIds.GitHubNameLabel;

			FontSize = _nameLabelHeight - 6;
			FontFamily = FontFamilyConstants.RobotoMedium;

			this.Bind(nameof(SettingsViewModel.GitHubNameLabelText))
				.Bind(IsVisibleProperty, nameof(SettingsViewModel.IsNotAuthenticating))
				.DynamicResource(TextColorProperty, nameof(BaseTheme.GitHubHandleColor));
		}
	}

	sealed class AliasLabel : Label
	{
		public AliasLabel()
		{
			this.Center();
			Opacity = 0.6;

			AutomationId = SettingsPageAutomationIds.GitHubAliasLabel;

			FontSize = _aliasLabelHeight - 4;
			FontFamily = FontFamilyConstants.RobotoRegular;

			this.Bind(nameof(SettingsViewModel.GitHubAliasLabelText))
				.Bind(IsVisibleProperty, nameof(SettingsViewModel.IsAliasLabelVisible))
				.DynamicResource(TextColorProperty, nameof(BaseTheme.GitHubHandleColor));
		}
	}

	sealed class TryDemoButton : Label
	{
		public TryDemoButton()
		{
			this.Center();
			Opacity = 0.6;

			AutomationId = SettingsPageAutomationIds.TryDemoButton;

			FontSize = _aliasLabelHeight - 4;
			FontFamily = FontFamilyConstants.RobotoRegular;

			Text = GitHubLoginButtonConstants.TryDemo;

			GestureRecognizers.Add(new TapGestureRecognizer()
				.Bind(TapGestureRecognizer.CommandProperty, nameof(SettingsViewModel.HandleDemoButtonTappedCommand))
				.Bind(TapGestureRecognizer.CommandParameterProperty,
					getter: static (Label label) => label.Text,
					source: this));

			this.Bind(IsVisibleProperty, nameof(SettingsViewModel.IsDemoButtonVisible))
				.Bind(TextProperty, nameof(SettingsViewModel.TryDemoButtonText))
				.DynamicResources(
					(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor)),
					(TextColorProperty, nameof(BaseTheme.GitHubHandleColor)));
		}
	}

	sealed class IsAuthenticatingActivityIndicator : ActivityIndicator
	{
		public IsAuthenticatingActivityIndicator()
		{
			AutomationId = SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator;

			HeightRequest = _nameLabelHeight;
			WidthRequest = _nameLabelHeight;

			this.Center()
				.Bind(IsVisibleProperty, nameof(SettingsViewModel.IsAuthenticating))
				.Bind(IsRunningProperty, nameof(SettingsViewModel.IsAuthenticating))
				.DynamicResource(ColorProperty, nameof(BaseTheme.ActivityIndicatorColor));
		}
	}
}