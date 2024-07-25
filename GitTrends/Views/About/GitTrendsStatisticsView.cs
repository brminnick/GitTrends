using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using CommunityToolkit.Maui.Markup;
using GitTrends.Resources;
using SkiaSharp.Views.Maui.Controls;
using static GitTrends.MauiService;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

class GitTrendsStatisticsView : HorizontalStackLayout
{
	const int _separatorWidth = 2;

	public GitTrendsStatisticsView()
	{
		this.Center();

		Spacing = 12;

		Children.Add(new StatisticsGrid(AboutPageConstants.Watching, "unique_views.svg", AboutPageAutomationIds.WatchersLabel, nameof(AboutViewModel.Watchers)));
		Children.Add(new DashedLineSeparator());
		Children.Add(new StatisticsGrid(AboutPageConstants.Stars, "star.svg", AboutPageAutomationIds.StarsLabel, nameof(AboutViewModel.Stars)));
		Children.Add(new DashedLineSeparator());
		Children.Add(new StatisticsGrid(AboutPageConstants.Forks, "repo_forked.svg", AboutPageAutomationIds.ForksLabel, nameof(AboutViewModel.Forks)));
	}

	class StatisticsGrid : Grid
	{
		public StatisticsGrid(in string title, in string svgFileName, in string automationId, in string bindingPath)
		{
			RowDefinitions = Rows.Define(
				(Row.Title, IsSmallScreen ? 12 : 16),
				(Row.Number, IsSmallScreen ? 16 : 20));

			Children.Add(new StatsTitleLayout(title, svgFileName, AppResources.GetResource<Color>(nameof(BaseTheme.SettingsLabelTextColor)))
				.Row(Row.Title));

			Children.Add(new StatisticsLabel(automationId)
				.Row(Row.Number)
				.Bind<Label, long?, string>(Label.TextProperty, bindingPath, BindingMode.OneTime, convert: static number => number.HasValue ? number.Value.ToAbbreviatedText() : "-"));
		}

		enum Row { Title, Number }

		class StatsTitleLayout : StackLayout
		{
			public StatsTitleLayout(in string text, in string svgFileName, in Color textColor)
			{
				Spacing = 2;
				Orientation = StackOrientation.Horizontal;

				HorizontalOptions = LayoutOptions.Center;

				Children.Add(new AboutPageSvgImage(svgFileName, textColor));
				Children.Add(new StatsTitleLabel(text));
			}

			class AboutPageSvgImage : SvgImage
			{
				public AboutPageSvgImage(in string svgFileName, in Color textColor) : base(svgFileName, textColor, 12, 12)
				{
					HorizontalOptions = LayoutOptions.End;
					VerticalOptions = LayoutOptions.Center;
				}
			}

			class StatsTitleLabel : Label
			{
				public StatsTitleLabel(in string text)
				{
					Text = text;
					LineBreakMode = LineBreakMode.TailTruncation;

					this.TextStart().TextCenterVertical().Fill().Font(FontFamilyConstants.RobotoMedium, IsSmallScreen ? 8 : 12).DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
				}
			}
		}

		class StatisticsLabel : Label
		{
			public StatisticsLabel(in string automationId)
			{
				AutomationId = automationId;

				FontSize = IsSmallScreen ? 12 : 16;
				FontFamily = FontFamilyConstants.RobotoRegular;

				this.Center().TextCenter().DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
			}
		}
	}

	class DashedLineSeparator : SKCanvasView
	{
		public DashedLineSeparator()
		{
			WidthRequest = _separatorWidth;
			PaintSurface += HandlePaintSurface;
		}

		static void HandlePaintSurface(object? sender, SKPaintSurfaceEventArgs e)
		{
			var imageInfo = e.Info;
			var canvas = e.Surface.Canvas;

			canvas.Clear();

			using var paintDashedLines = new SKPaint();
			paintDashedLines.Style = SKPaintStyle.Stroke;
			paintDashedLines.Color = Color.FromArgb(BaseTheme.LightTealColorHex).ToSKColor();
			paintDashedLines.StrokeWidth = _separatorWidth;
			paintDashedLines.StrokeCap = SKStrokeCap.Butt;
			paintDashedLines.PathEffect = SKPathEffect.CreateDash([8f, 8f], 0f);

			canvas.DrawLine(imageInfo.Width / 2.0f, 0, imageInfo.Width / 2.0f, imageInfo.Height, paintDashedLines);
		}
	}
}