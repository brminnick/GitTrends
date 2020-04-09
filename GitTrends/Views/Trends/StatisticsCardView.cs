using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class StatisticsCard : PancakeView
    {
        public StatisticsCard(in string title, in string svgImage, in string svgColorTheme, in string textBinding, in string tapGestureBinding, in string cardAutomationId, in string statisticsTextAutomationId)
        {
            Padding = new Thickness(16, 12);
            Content = new StatisticsCardContent(title, textBinding, svgImage, svgColorTheme, statisticsTextAutomationId);
            HasShadow = false;
            CornerRadius = 4;
            BorderThickness = 2;
            AutomationId = cardAutomationId;

            this.BindTapGesture(tapGestureBinding);

            SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
        }

        enum Row { Title, Number }
        enum Column { Stats, Icon }

        class StatisticsCardContent : Grid
        {
            public StatisticsCardContent(in string title, in string textBinding, in string svgImage, in string svgColorTheme, in string statisticsTextAutomationId)
            {
                RowSpacing = 0;
                ColumnSpacing = 0;
                VerticalOptions = LayoutOptions.FillAndExpand;
                HorizontalOptions = LayoutOptions.FillAndExpand;

                RowDefinitions = Rows.Define(
                    (Row.Title, StarGridLength(1)),
                    (Row.Number, StarGridLength(2)));

                ColumnDefinitions = Columns.Define(
                    (Column.Stats, StarGridLength(1)),
                    (Column.Icon, AbsoluteGridLength(32)));

                Children.Add(new PrimaryColorLabel(14, title).Row(Row.Title).Column(Column.Stats));
                Children.Add(new TrendsStatisticsLabel(34, textBinding, statisticsTextAutomationId).Row(Row.Number).Column(Column.Stats).ColumnSpan(2));
                Children.Add(new RepositoryStatSVGImage(svgImage, svgColorTheme, 32, 32).Row(Row.Title).Column(Column.Icon).RowSpan(2));
            }
        }

        class RepositoryStatSVGImage : SvgImage
        {
            public RepositoryStatSVGImage(in string svgFileName, string baseThemeColor, in double width, in double height)
                : base(svgFileName, () => (Color)Application.Current.Resources[baseThemeColor], width, height)
            {
                VerticalOptions = LayoutOptions.CenterAndExpand;
                HorizontalOptions = LayoutOptions.EndAndExpand;
            }
        }

        class TrendsStatisticsLabel : Label
        {
            public TrendsStatisticsLabel(in double fontSize, in string textBinding, in string automationId)
            {
                MaxLines = 1;
                FontSize = fontSize;
                FontFamily = FontFamilyConstants.RobotoMedium;
                LineBreakMode = LineBreakMode.TailTruncation;

                VerticalOptions = LayoutOptions.Start;
                HorizontalOptions = LayoutOptions.FillAndExpand;
                VerticalTextAlignment = TextAlignment.Start;
                HorizontalTextAlignment = TextAlignment.Start;

                Margin = new Thickness(0, 4, 0, 0);

                Opacity = 0.87;

                AutomationId = automationId;

                this.SetBinding(TextProperty, textBinding);
                this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.AreStatisticsVisible));

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
            }
        }

        class PrimaryColorLabel : Label
        {
            public PrimaryColorLabel(in double fontSize, in string text)
            {
                Text = text;
                FontSize = fontSize;
                Opacity = 0.6;
                LineBreakMode = LineBreakMode.TailTruncation;
                HorizontalTextAlignment = TextAlignment.Start;
                VerticalOptions = LayoutOptions.Start;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
            }
        }
    }
}
