using System;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    abstract class BaseOnboardingContentPage : ContentPage
    {
        protected const string TealBackgroundColorHex = "338F82";
        protected const string CoralBackgroundColorHex = "F97B4F";

        readonly static WeakEventManager _skipButtonTappedEventManager = new WeakEventManager();

        public BaseOnboardingContentPage(GitHubAuthenticationService gitHubAuthenticationService, string backgroundColorHex, string nextButtonText, int carouselPositionIndex)
        {
            BackgroundColor = Color.FromHex(backgroundColorHex);

            var imageView = CreateImageView();
            imageView.Margin = new Thickness(30);

            var descriptionLayout = new StackLayout
            {
                Spacing = 24,
                Margin = new Thickness(30, 10),
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
                    new NextButton(nextButtonText, gitHubAuthenticationService).Row(Row.Indicator).Column(Column.Button),
                }
            };
        }

        public static event EventHandler SkipButtonTapped
        {
            add => _skipButtonTappedEventManager.AddEventHandler(value);
            remove => _skipButtonTappedEventManager.RemoveEventHandler(value);
        }

        enum Row { Image, Description, Indicator }
        enum Column { Indicator, Button }

        protected abstract View CreateImageView();
        protected abstract TitleLabel CreateDescriptionTitleLabel();
        protected abstract View CreateDescriptionBodyView();

        static void OnSkipButtonTapped() => _skipButtonTappedEventManager.HandleEvent(null, EventArgs.Empty, nameof(SkipButtonTapped));

        class NextButton : Button
        {
            readonly GitHubAuthenticationService _gitHubAuthenticationService;

            public NextButton(in string text, GitHubAuthenticationService gitHubAuthenticationService)
            {
                _gitHubAuthenticationService = gitHubAuthenticationService;

                Padding = 0;
                Margin = new Thickness(0, 0, Device.RuntimePlatform is Device.iOS ? 30 : 0, 0);
                TextColor = Color.White;
                HorizontalOptions = LayoutOptions.End;
                Text = text;
                BackgroundColor = Color.Transparent;
                FontFamily = FontFamilyConstants.RobotoBold;
                AutomationId = OnboardingAutomationIds.NextButon;

                this.SetBinding(IsVisibleProperty, nameof(OnboardingViewModel.IsDemoButtonVisible));

                Clicked += HandleNextButtonClicked;
            }

            async void HandleNextButtonClicked(object sender, EventArgs e)
            {
                if (Text is OnboardingConstants.SkipText)
                {
                    OnSkipButtonTapped();
                }
                else if (Text is OnboardingConstants.TryDemoText)
                {
                    FirstRunService.IsFirstRun = false;

                    _gitHubAuthenticationService.ActivateDemoUser();

                    await Navigation.PopModalAsync();
                }
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
                FontFamily = FontFamilyConstants.RobotoBold;
            }
        }

        protected class BodyLabel : Label
        {
            public BodyLabel(in string text)
            {
                Text = text;
                FontSize = 16;
                TextColor = Color.White;
                FontFamily = FontFamilyConstants.RobotoRegular;
                VerticalTextAlignment = TextAlignment.Center;
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
                Count = 4;
                HorizontalOptions = LayoutOptions.Start;
            }
        }
    }
}
