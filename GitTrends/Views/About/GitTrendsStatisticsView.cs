using System;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static GitTrends.XamarinFormsService;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    class GitTrendsStatisticsView : StackLayout
    {
        const int _separatorWidth = 2;

        public GitTrendsStatisticsView()
        {
            this.Center();

            Spacing = 12;

            Orientation = StackOrientation.Horizontal;

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

                Children.Add(new StatsTitleLayout(title, svgFileName, () => (Color)Application.Current.Resources[nameof(BaseTheme.SettingsLabelTextColor)])
                                .Row(Row.Title));

                Children.Add(new StatisticsLabel(automationId)
                                .Row(Row.Number)
                                .Bind<Label, long?, string>(Label.TextProperty, bindingPath, BindingMode.OneTime, convert: number => number.HasValue ? number.Value.ToAbbreviatedText() : "-"));
            }

            enum Row { Title, Number }

            class StatsTitleLayout : StackLayout
            {
                public StatsTitleLayout(in string text, in string svgFileName, in Func<Color> getTextColor)
                {
                    Spacing = 2;
                    Orientation = StackOrientation.Horizontal;

                    HorizontalOptions = LayoutOptions.Center;

                    Children.Add(new AboutPageSvgImage(svgFileName, getTextColor));
                    Children.Add(new StatsTitleLabel(text));
                }

                class AboutPageSvgImage : SvgImage
                {
                    public AboutPageSvgImage(in string svgFileName, in Func<Color> getTextColor) : base(svgFileName, getTextColor, 12, 12)
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

                        this.TextStart().TextCenterVertical().FillExpand().Font(FontFamilyConstants.RobotoMedium, IsSmallScreen ? 8 : 12).DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
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

            void HandlePaintSurface(object sender, SKPaintSurfaceEventArgs e)
            {
                var imageInfo = e.Info;
                var canvas = e.Surface.Canvas;

                canvas.Clear();

                using var paintDashedLines = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = Color.FromHex(BaseTheme.LightTealColorHex).ToSKColor(),
                    StrokeWidth = _separatorWidth,
                    StrokeCap = SKStrokeCap.Butt,
                    PathEffect = SKPathEffect.CreateDash(new[] { 8f, 8f }, 0f)
                };

                canvas.DrawLine(imageInfo.Width / 2, 0, imageInfo.Width / 2, imageInfo.Height, paintDashedLines);
            }
        }
    }
}
