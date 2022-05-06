using GitTrends.Mobile.Common;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
	class StarsStatisticsGrid : Grid
	{
		const int _textSize = 24;
		const int _textRowHeight = _textSize + 8;

		public StarsStatisticsGrid()
		{
			this.FillExpand()
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

			Children.Add(new StarsStatisticsLabel("TOTAL", _textSize)
							.Row(Row.Total).ColumnSpan(All<Column>()));

			Children.Add(new StarSvg()
							.Row(Row.Stars).Column(Column.LeftStar));

			Children.Add(new StarsStatisticsLabel(48) { AutomationId = TrendsPageAutomationIds.StarsStatisticsLabel }
							.Row(Row.Stars).Column(Column.Text)
							.CenterExpand()
							.Bind(IsVisibleProperty, nameof(TrendsViewModel.IsStarsChartVisible))
							.Bind<Label, double, string>(Label.TextProperty, nameof(TrendsViewModel.TotalStars), convert: totalStars => totalStars.ToAbbreviatedText()));

			Children.Add(new ActivityIndicator()
							.Row(Row.Stars).Column(Column.Text)
							.CenterExpand()
							.DynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.ActivityIndicatorColor))
							.Bind(IsVisibleProperty, nameof(TrendsViewModel.IsFetchingStarsData))
							.Bind(ActivityIndicator.IsRunningProperty, nameof(TrendsViewModel.IsFetchingStarsData)));

			Children.Add(new StarSvg()
							.Row(Row.Stars).Column(Column.RightStar));

			Children.Add(new StarsStatisticsLabel(_textSize) { AutomationId = TrendsPageAutomationIds.StarsHeaderMessageLabel }
							.Row(Row.Message).ColumnSpan(All<Column>())
							.Bind(Label.TextProperty, nameof(TrendsViewModel.StarsHeaderMessageText)));

			Children.Add(new SeparatorLine()
							.Row(Row.BottomLine).ColumnSpan(All<Column>()));
		}

		enum Row { TopLine, Total, Stars, Message, BottomLine }
		enum Column { LeftStar, Text, RightStar }

		class StarSvg : SvgImage
		{
			public StarSvg() : base("star.svg", () => Color.White, 44, 44)
			{
				this.Center();
			}
		}

		class SeparatorLine : BoxView
		{
			public SeparatorLine() => BackgroundColor = Color.White;
		}

		class StarsStatisticsLabel : Label
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
				TextColor = Color.FromHex(DarkTheme.PageBackgroundColorHex);
				FontFamily = FontFamilyConstants.RobotoBold;
			}
		}
	}
}