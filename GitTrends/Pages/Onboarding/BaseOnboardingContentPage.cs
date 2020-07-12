using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public abstract class BaseOnboardingContentPage : BaseContentPage
    {
        protected BaseOnboardingContentPage(in IAnalyticsService analyticsService,
                                            in IMainThread mainThread,
                                            in Color backgroundColor,
                                            in string nextButtonText,
                                            in int carouselPositionIndex) : base(analyticsService, mainThread)
        {
            //Don't Use BaseTheme.PageBackgroundColor
            RemoveDynamicResource(BackgroundColorProperty);

            BackgroundColor = backgroundColor;

            var descriptionLayout = new StackLayout
            {
                Margin = new Thickness(32, 8),
                Spacing = 16,
                Children =
                {
                    CreateDescriptionTitleLabel(),
                    CreateDescriptionBodyView()
                }
            };

            Content = new Grid
            {
                RowDefinitions = Rows.Define(
                    (Row.Image, StarGridLength(GetImageRowStarHeight())),
                    (Row.Description, StarGridLength(GetDescriptionRowStarHeight())),
                    (Row.Indicator, AbsoluteGridLength(44))),

                ColumnDefinitions = Columns.Define(
                    (Column.Indicator, StarGridLength(1)),
                    (Column.Button, StarGridLength(1))),

                Children =
                {
                    new OpacityOverlay().Row(Row.Image).ColumnSpan(All<Column>()),
                    CreateImageView().Row(Row.Image).ColumnSpan(All<Column>()).Margin(Device.RuntimePlatform is Device.iOS ? new Thickness(32, 44 + 32, 32, 32) : new Thickness(32,16)),
                    descriptionLayout.Row(Row.Description).RowSpan(2).ColumnSpan(All<Column>()),
                    new OnboardingIndicatorView(carouselPositionIndex).Row(Row.Indicator).Column(Column.Indicator),
                    new NextLabel(nextButtonText).Row(Row.Indicator).Column(Column.Button),
                }
            };
        }

        enum Row { Image, Description, Indicator }
        enum Column { Indicator, Button }

        protected OnboardingViewModel ViewModel => (OnboardingViewModel)BindingContext;

        protected abstract View CreateImageView();
        protected abstract TitleLabel CreateDescriptionTitleLabel();
        protected abstract View CreateDescriptionBodyView();

        static int GetImageRowStarHeight()
        {
            if (ScreenHeight < 700)
                return 8;

            return Device.RuntimePlatform is Device.iOS ? 3 : 11;
        }

        static int GetDescriptionRowStarHeight()
        {
            if (ScreenHeight < 700)
                return 9;

            return Device.RuntimePlatform is Device.iOS ? 2 : 9;
        }

        class NextLabel : Label
        {
            public NextLabel(in string text)
            {
                Text = text;
                FontSize = 15;
                TextColor = Color.White;
                BackgroundColor = Color.Transparent;
                FontFamily = FontFamilyConstants.RobotoRegular;

                Opacity = 0.8;

                Margin = new Thickness(0, 0, 30, 0);

                HorizontalOptions = LayoutOptions.End;
                VerticalOptions = LayoutOptions.Center;

                AutomationId = OnboardingAutomationIds.NextButon;

                GestureRecognizers.Add(new TapGestureRecognizer { CommandParameter = text }
                                        .Bind(TapGestureRecognizer.CommandProperty, nameof(OnboardingViewModel.DemoButtonCommand)));

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
                FontSize = 15;
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
