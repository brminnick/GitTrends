using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Resources;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;
using static GitTrends.MauiService;

namespace GitTrends;

class GitTrendsStatisticsView : HorizontalStackLayout
{
	const int _separatorWidth = 2;

	public GitTrendsStatisticsView(in IDeviceInfo deviceInfo)
	{
		this.Center();

		Spacing = 12;

		Children.Add(new StatisticsGrid(deviceInfo, AboutPageConstants.Watching, "unique_views.svg", AboutPageAutomationIds.WatchersLabel, nameof(AboutViewModel.Watchers)));
		Children.Add(new DashedLineSeparator());
		Children.Add(new StatisticsGrid(deviceInfo, AboutPageConstants.Stars, "star.svg", AboutPageAutomationIds.StarsLabel, nameof(AboutViewModel.Stars)));
		Children.Add(new DashedLineSeparator());
		Children.Add(new StatisticsGrid(deviceInfo, AboutPageConstants.Forks, "repo_forked.svg", AboutPageAutomationIds.ForksLabel, nameof(AboutViewModel.Forks)));
	}

	class StatisticsGrid : Grid
	{
		public StatisticsGrid(in IDeviceInfo deviceInfo, in string title, in string svgFileName, in string automationId, in string bindingPath)
		{
			RowDefinitions = Rows.Define(
				(Row.Title, IsSmallScreen ? 12 : 16),
				(Row.Number, IsSmallScreen ? 16 : 20));

			Children.Add(new StatsTitleLayout(deviceInfo, title, svgFileName, AppResources.GetResource<Color>(nameof(BaseTheme.SettingsLabelTextColor)))
				.Row(Row.Title));

			Children.Add(new StatisticsLabel(automationId)
				.Row(Row.Number)
				.Bind<Label, long?, string>(Label.TextProperty, bindingPath, BindingMode.OneTime, convert: static number => number.HasValue ? number.Value.ToAbbreviatedText() : "-"));
		}

		enum Row { Title, Number }

		sealed class StatsTitleLayout : HorizontalStackLayout
		{
			public StatsTitleLayout(in IDeviceInfo deviceInfo, in string text, in string svgFileName, in Color textColor)
			{
				Spacing = 2;

				HorizontalOptions = LayoutOptions.Center;

				Children.Add(new AboutPageSvgImage(deviceInfo, svgFileName, textColor));
				Children.Add(new StatsTitleLabel(text));
			}

			sealed class AboutPageSvgImage : SvgImage
			{
				public AboutPageSvgImage(in IDeviceInfo deviceInfo, in string svgFileName, Color textColor) : base(deviceInfo, svgFileName, () => textColor, 12, 12)
				{
					HorizontalOptions = LayoutOptions.End;
					VerticalOptions = LayoutOptions.Center;
				}
			}

			sealed class StatsTitleLabel : Label
			{
				public StatsTitleLabel(in string text)
				{
					Text = text;
					LineBreakMode = LineBreakMode.TailTruncation;

					this.TextStart().TextCenterVertical().Fill().Font(FontFamilyConstants.RobotoMedium, IsSmallScreen ? 8 : 12).DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
				}
			}
		}

		sealed class StatisticsLabel : Label
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

	sealed class DashedLineSeparator : SKCanvasView
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