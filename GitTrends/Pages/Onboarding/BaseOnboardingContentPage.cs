using GitTrends.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public abstract class BaseOnboardingContentPage : BaseContentPage
    {
        protected const string TealBackgroundColorHex = "338F82";
        protected const string CoralBackgroundColorHex = "F97B4F";

        protected BaseOnboardingContentPage(AnalyticsService analyticsService, in string backgroundColorHex, in string nextButtonText, in int carouselPositionIndex) : base(analyticsService)
        {
            //Don't Use BaseTheme.PageBackgroundColor
            RemoveDynamicResource(BackgroundColorProperty);

            BackgroundColor = Color.FromHex(backgroundColorHex);

            var imageView = CreateImageView();
            imageView.Margin = new Thickness(32);

            var descriptionLayout = new StackLayout
            {
                Margin = new Thickness(32, 16),
                Spacing = 24,
                Children =
                {
                    CreateDescriptionTitleLabel(),
                    CreateDescriptionBodyView()
                }
            };

            Content = new Grid
            {
                RowDefinitions = Rows.Define(
                    (Row.Image, StarGridLength(Device.RuntimePlatform is Device.iOS ? 9 : 5)),
                    (Row.Description, StarGridLength(Device.RuntimePlatform is Device.iOS ? 6 : 4)),
                    (Row.Indicator, StarGridLength(1))),

                ColumnDefinitions = Columns.Define(
                    (Column.Indicator, StarGridLength(1)),
                    (Column.Button, StarGridLength(1))),

                Children =
                {
                    new OpacityOverlay().Row(Row.Image).ColumnSpan(All<Column>()),
                    imageView.Row(Row.Image).ColumnSpan(All<Column>()),
                    descriptionLayout.Row(Row.Description).ColumnSpan(All<Column>()),
                    new OnboardingIndicatorView(carouselPositionIndex).Row(Row.Indicator).Column(Column.Indicator),
                    new NextButton(nextButtonText).Row(Row.Indicator).Column(Column.Button),
                }
            };
        }

        enum Row { Image, Description, Indicator }
        enum Column { Indicator, Button }

        protected abstract View CreateImageView();
        protected abstract TitleLabel CreateDescriptionTitleLabel();
        protected abstract View CreateDescriptionBodyView();

        class NextButton : Button
        {
            public NextButton(in string text)
            {
                Padding = 0;
                Margin = new Thickness(0, 0, Device.RuntimePlatform is Device.iOS ? 32 : 0, 0);
                TextColor = Color.White;
                HorizontalOptions = LayoutOptions.End;
                VerticalOptions = LayoutOptions.Center;
                CommandParameter = Text = text;
                BackgroundColor = Color.Transparent;
                FontFamily = FontFamilyConstants.RobotoBold;
                AutomationId = OnboardingAutomationIds.NextButon;

                this.SetBinding(CommandProperty, nameof(OnboardingViewModel.DemoButtonCommand));
                this.SetBinding(IsVisibleProperty, nameof(OnboardingViewModel.IsDemoButtonVisible));
            }
        }

        protected class BodySvg : SvgImage
        {
            public BodySvg(in string svgFileName) : base(svgFileName, () => Color.White, 24, 24)
            {

            }
        }

        protected class TitleLabel : Label
        {
            public TitleLabel(in string text)
            {
                Text = text;
                FontSize = 34;
                TextColor = Color.White;
                LineHeight = 1.12;
                FontFamily = FontFamilyConstants.RobotoBold;
                AutomationId = OnboardingAutomationIds.TitleLabel;
            }
        }

        protected class BodyLabel : Label
        {
            public BodyLabel(in string text)
            {
                Text = text;
                FontSize = 16;
                TextColor = Color.White;
                LineHeight = 1.021;
                LineBreakMode = LineBreakMode.WordWrap;
                FontFamily = FontFamilyConstants.RobotoRegular;
                VerticalTextAlignment = TextAlignment.Start;
            }
        }

        class OpacityOverlay : BoxView
        {
            public OpacityOverlay() => BackgroundColor = Color.White.MultiplyAlpha(0.25);
        }

        class OnboardingIndicatorView : IndicatorView
        {
            public OnboardingIndicatorView(in int position)
            {
                Position = position;
                SelectedIndicatorColor = Color.White;
                IndicatorColor = Color.White.MultiplyAlpha(0.25);
                Margin = new Thickness(30, 0, 0, 0);
                HorizontalOptions = LayoutOptions.Start;
                AutomationId = OnboardingAutomationIds.PageIndicator;

                SetBinding(CountProperty, new Binding(nameof(OnboardingCarouselPage.PageCount),
                                                        source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(OnboardingCarouselPage))));
            }
        }
    }
}
