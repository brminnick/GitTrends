using CommunityToolkit.Maui.Markup;
using GitTrends.Resources;
using Sharpnado.MaterialFrame;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

class StatisticsCard : MaterialFrame
{
	public static readonly BindableProperty IsSeriesVisibleProperty = BindableProperty.Create(nameof(IsSeriesVisible), typeof(bool), typeof(StatisticsCard), false);
	public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(StatisticsCard), string.Empty);

	public StatisticsCard(in string title,
		in string svgImage,
		in string svgColorTheme,
		in string cardAutomationId,
		in string statisticsTextAutomationId,
		in IDeviceInfo deviceInfo)
	{
		Elevation = 4;

		Padding = new Thickness(16, 12);
		Content = new StatisticsCardContent(title, svgImage, svgColorTheme, statisticsTextAutomationId, this, deviceInfo);
		CornerRadius = 4;
		AutomationId = cardAutomationId;

		this.DynamicResources((BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor)),
			(MaterialThemeProperty, nameof(BaseTheme.MaterialFrameTheme)));
	}

	enum Row { Title, Number }
	enum Column { Stats, Icon }

	public bool IsSeriesVisible
	{
		get => (bool)GetValue(IsSeriesVisibleProperty);
		set => SetValue(IsSeriesVisibleProperty, value);
	}

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	sealed class StatisticsCardContent : Grid
	{
		readonly RepositoryStatSVGImage _svgImage;

		public StatisticsCardContent(in string title,
			in string svgImage,
			in string svgColorTheme,
			in string statisticsTextAutomationId,
			in StatisticsCard statisticsCard,
			in IDeviceInfo deviceInfo)
		{
			RowSpacing = 0;
			ColumnSpacing = 0;
			VerticalOptions = LayoutOptions.Fill;
			HorizontalOptions = LayoutOptions.Fill;

			RowDefinitions = Rows.Define(
				(Row.Title, Stars(1)),
				(Row.Number, Stars(2)));

			ColumnDefinitions = Columns.Define(
				(Column.Stats, Stars(1)),
				(Column.Icon, 32));

			Children.Add(new PrimaryColorLabel(14, title)
				.Row(Row.Title).Column(Column.Stats));

			Children.Add(new TrendsStatisticsLabel(34, statisticsTextAutomationId)
				.Row(Row.Number).Column(Column.Stats).ColumnSpan(2)
				.Bind(Label.TextProperty, nameof(Text), source: statisticsCard)
				.Bind<TrendsStatisticsLabel, bool, bool>(IsVisibleProperty,
					nameof(TrendsViewModel.IsFetchingViewsClonesData),
					convert: static isFetchingData => !isFetchingData,
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel))));

			Children.Add(new RepositoryStatSVGImage(svgImage, svgColorTheme, deviceInfo).Assign(out _svgImage)
				.Row(Row.Title).Column(Column.Icon).RowSpan(2)
				.Bind(SvgImage.GetSvgColorProperty,
					getter: static vm => vm.IsSeriesVisible,
					convert: convertIsSeriesVisible,
					source: statisticsCard));

			Func<Color> convertIsSeriesVisible(bool isVisible) => isVisible ? () => _svgImage.DefaultColor : () => Colors.Gray;
		}

		sealed class RepositoryStatSVGImage : SvgImage
		{
			public RepositoryStatSVGImage(in string svgFileName, string baseThemeColor, in IDeviceInfo deviceInfo)
				: base(deviceInfo, svgFileName, () => AppResources.GetResource<Color>(baseThemeColor), 32, 32)
			{
				VerticalOptions = LayoutOptions.Center;
				HorizontalOptions = LayoutOptions.End;

				DefaultColor = AppResources.GetResource<Color>(baseThemeColor);
			}

			public Color DefaultColor { get; }
		}

		class TrendsStatisticsLabel : Label
		{
			public TrendsStatisticsLabel(in double fontSize, in string automationId)
			{
				MaxLines = 1;
				FontSize = fontSize;
				FontFamily = FontFamilyConstants.RobotoMedium;
				LineBreakMode = LineBreakMode.TailTruncation;

				VerticalOptions = LayoutOptions.Start;
				HorizontalOptions = LayoutOptions.Fill;
				VerticalTextAlignment = TextAlignment.Start;
				HorizontalTextAlignment = TextAlignment.Start;

				Margin = new Thickness(0, 4, 0, 0);

				Opacity = 0.87;

				AutomationId = automationId;

				this.DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
			}
		}

		sealed class PrimaryColorLabel : Label
		{
			public PrimaryColorLabel(in double fontSize, in string text)
			{
				Text = text;
				FontSize = fontSize;
				Opacity = 0.6;
				LineBreakMode = LineBreakMode.TailTruncation;
				HorizontalTextAlignment = TextAlignment.Start;
				VerticalOptions = LayoutOptions.Start;

				this.DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
			}
		}
	}
}