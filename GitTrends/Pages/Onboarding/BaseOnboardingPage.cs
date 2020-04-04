using System;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    abstract class BaseOnboardingPage : ContentPage
    {
        readonly static WeakEventManager _skipButtonTappedEventManager = new WeakEventManager();

        public BaseOnboardingPage(GitHubAuthenticationService gitHubAuthenticationService, string backgroundColorHex, string nextButtonText, int carouselPositionIndex)
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
                    (Row.ImageRow, StarGridLength(9)),
                    (Row.DescriptionRow, StarGridLength(6)),
                    (Row.IndicatorRow, StarGridLength(1))),

                ColumnDefinitions = Columns.Define(
                    (Column.IndicatorColumn, StarGridLength(1)),
                    (Column.ButtonColumn, StarGridLength(1))),

                Children =
                {
                    new OpacityOverlay().Row(Row.ImageRow).ColumnSpan(All<Column>()),
                    imageView.Row(Row.ImageRow).ColumnSpan(All<Column>()),
                    descriptionLayout.Row(Row.DescriptionRow).ColumnSpan(All<Column>()),
                    new OnboardingIndicatorView(carouselPositionIndex).Row(Row.IndicatorRow).Column(Column.IndicatorColumn),
                    new NextButton(nextButtonText, gitHubAuthenticationService).Row(Row.IndicatorRow).Column(Column.ButtonColumn),
                }
            };
        }

        public static event EventHandler SkipButtonTapped
        {
            add => _skipButtonTappedEventManager.AddEventHandler(value);
            remove => _skipButtonTappedEventManager.RemoveEventHandler(value);
        }

        enum Row { ImageRow, DescriptionRow, IndicatorRow }
        enum Column { IndicatorColumn, ButtonColumn }

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

                Margin = new Thickness(0, 0, 30, 0);
                TextColor = Color.White;
                HorizontalOptions = LayoutOptions.End;
                Text = text;
                BackgroundColor = Color.Transparent;
                FontFamily = FontFamilyConstants.RobotoBold;
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
            public OpacityOverlay() => BackgroundColor = Color.FromRgba(255, 255, 255, 0.25);
        }

        class OnboardingIndicatorView : IndicatorView
        {
            public OnboardingIndicatorView(int position)
            {
                Margin = new Thickness(30, 0, 0, 0);
                Count = 4;
                HorizontalOptions = LayoutOptions.Start;

                Position = position;
            }
        }
    }
}
