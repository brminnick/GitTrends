using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

class StarsStatisticsGrid : Grid
{
	const int _textSize = 24;
	const int _textRowHeight = _textSize + 8;

	public StarsStatisticsGrid(in IDeviceInfo deviceInfo)
	{
		this.Fill()
			.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardStarsStatsIconColor));

		Padding = 20;
		RowSpacing = 8;

		RowDefinitions = Rows.Define(
			(Row.TopLine, 1),
			(Row.Total, _textRowHeight),
			(Row.Stars, Star),
			(Row.Message, _textRowHeight),
			(Row.BottomLine, 1));

		ColumnDefinitions = Columns.Define(
			(Column.LeftStar, Star),
			(Column.Text, Stars(3)),
			(Column.RightStar, Star));

		Children.Add(new SeparatorLine()
			.Row(Row.TopLine).ColumnSpan(All<Column>()));

		Children.Add(new StarsStatisticsLabel(EmptyDataViewService.GetStarsHeaderTitleText(), _textSize)
			.Row(Row.Total).ColumnSpan(All<Column>()));

		Children.Add(new StarSvg(deviceInfo)
			.Row(Row.Stars).Column(Column.LeftStar));

		Children.Add(new StarsStatisticsLabel(48) { AutomationId = TrendsPageAutomationIds.StarsStatisticsLabel }
			.Row(Row.Stars).Column(Column.Text)
			.Center()
			.Bind<Label, bool, bool>(IsVisibleProperty,
				nameof(TrendsViewModel.IsFetchingStarsData),
				convert: static isFetchingStarsData => !isFetchingStarsData,
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.Bind<Label, double, string>(Label.TextProperty,
				nameof(TrendsViewModel.TotalStars),
				convert: static totalStars => totalStars.ToAbbreviatedText(),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel))));

		Children.Add(new ActivityIndicator()
			.Row(Row.Stars).Column(Column.Text)
			.Center()
			.DynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.ActivityIndicatorColor))
			.Bind(IsVisibleProperty,
				nameof(TrendsViewModel.IsFetchingStarsData),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.Bind(ActivityIndicator.IsRunningProperty,
				nameof(TrendsViewModel.IsFetchingStarsData),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel))));

		Children.Add(new StarSvg(deviceInfo)
			.Row(Row.Stars).Column(Column.RightStar));

		Children.Add(new StarsStatisticsLabel(_textSize) { AutomationId = TrendsPageAutomationIds.StarsHeaderMessageLabel }
			.Row(Row.Message).ColumnSpan(All<Column>())
			.Bind(Label.TextProperty,
				nameof(TrendsViewModel.StarsHeaderMessageText),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel))));

		Children.Add(new SeparatorLine()
			.Row(Row.BottomLine).ColumnSpan(All<Column>()));
	}

	enum Row { TopLine, Total, Stars, Message, BottomLine }
	enum Column { LeftStar, Text, RightStar }

	sealed class StarSvg : SvgImage
	{
		public StarSvg(in IDeviceInfo deviceInfo) : base(deviceInfo, "star.svg", () => Colors.White, 44, 44)
		{
			this.Center();
		}
	}

	sealed class SeparatorLine : BoxView
	{
		public SeparatorLine() => BackgroundColor = Colors.White;
	}

	sealed class StarsStatisticsLabel : Label
	{
		public StarsStatisticsLabel(in string text, in int fontSize) : this(fontSize)
		{
			Text = text;
		}

		public StarsStatisticsLabel(in int fontSize)
		{
			this.TextCenter();

			TextTransform = TextTransform.Uppercase;

			FontSize = fontSize;
			TextColor = Color.FromArgb(DarkTheme.PageBackgroundColorHex);
			FontFamily = FontFamilyConstants.RobotoBold;
		}
	}
}