using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using GitTrends.Mobile.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    class SplashScreenPage : BaseContentPage<SplashScreenViewModel>
    {
        readonly IEnumerator<string> _statusMessageEnumerator;
        readonly Image _gitTrendsImage;
        readonly Label _statusLabel;

        CancellationTokenSource? _animationCancellationToken;

        public SplashScreenPage(AnalyticsService analyticsService,
                                SplashScreenViewModel splashScreenViewModel)
            : base(splashScreenViewModel, analyticsService, shouldUseSafeArea: false)
        {
            //Remove BaseContentPageBackground
            RemoveDynamicResource(BackgroundColorProperty);
            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.GitTrendsImageBackgroundColor));

            ViewModel.InitializationComplete += HandleInitializationComplete;

            IEnumerable<string> statusMessageList = new[] { "Initializing", "Connecting to servers", "Initializing", "Connecting to servers", "Initializing", "Connecting to servers", "Still working on it", "Let's try it like this", "Maybe this", "Another try", "Hmmm, it shouldn't take this long", "Are you sure the internet connection is good?" };
            _statusMessageEnumerator = statusMessageList.GetEnumerator();
            _statusMessageEnumerator.MoveNext();

            _gitTrendsImage = new Image
            {
                AutomationId = SplashScreenPageAutomationIds.GitTrendsImage,
                Opacity = 0,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Aspect = Aspect.AspectFit
            };
            _gitTrendsImage.SetDynamicResource(Image.SourceProperty, nameof(BaseTheme.GitTrendsImageSource));

            _statusLabel = new Label
            {
                //Begin with Label off of the screen
                TranslationX = DeviceDisplay.MainDisplayInfo.Width / 2,
                Margin = new Thickness(10, 0),
                AutomationId = SplashScreenPageAutomationIds.StatusLabel,
                HorizontalTextAlignment = TextAlignment.Center,
            };
            _statusLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.SplashScreenStatusColor));

            var relativeLayout = new RelativeLayout();

            relativeLayout.Children.Add(_statusLabel,
                Constraint.RelativeToParent(parent => parent.Width / 2 - _statusLabel.GetWidth(parent) / 2),
                Constraint.RelativeToParent(parent => parent.Height - _statusLabel.GetHeight(parent) - (DeviceDisplay.MainDisplayInfo.Orientation is DisplayOrientation.Landscape ? 25 : 50)));

            relativeLayout.Children.Add(_gitTrendsImage,
                Constraint.RelativeToParent(parent => parent.Width / 2 - _gitTrendsImage.GetWidth(parent) / 2),
                Constraint.RelativeToParent(parent => parent.Height / 2 - _gitTrendsImage.GetHeight(parent) / 2));

            Content = relativeLayout;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ChangeLabelText(_statusMessageEnumerator.Current);

            _animationCancellationToken = new CancellationTokenSource();

            //Fade the Image Opacity to 1. Work around for https://github.com/xamarin/Xamarin.Forms/issues/8073
            var fadeImageTask = _gitTrendsImage.FadeTo(1, 1000, Easing.CubicIn);
            var pulseImageTask = PulseImage();

            //Slide status label into screen
            await _statusLabel.TranslateTo(-10, 0, 250, Easing.CubicOut);
            await _statusLabel.TranslateTo(0, 0, 250, Easing.CubicOut);

            //Wait for Image to reach an opacity of 1
            await Task.WhenAll(fadeImageTask, pulseImageTask);

            ViewModel.InitializeAppCommand.Execute(null);

            Animate(_animationCancellationToken.Token);
        }

        async void Animate(CancellationToken pulseCancellationToken)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                while (!pulseCancellationToken.IsCancellationRequested)
                {
                    var pulseImageTask = PulseImage();
                    await Task.Delay(TimeSpan.FromMilliseconds(400));

                    //Label leaves the screen
                    await _statusLabel.TranslateTo(10, 0, 100, Easing.CubicInOut);
                    await _statusLabel.TranslateTo(-DeviceDisplay.MainDisplayInfo.Width / 2, 0, 250, Easing.CubicIn);

                    //Move the label to the other side of the screen
                    _statusLabel.TranslationX = DeviceDisplay.MainDisplayInfo.Width / 2;

                    //Update Status Label Text
                    if (!_statusMessageEnumerator.MoveNext())
                    {
                        _statusMessageEnumerator.Reset();
                        _statusMessageEnumerator.MoveNext();
                    }
                    await ChangeLabelText(_statusMessageEnumerator.Current);

                    //Label reappears on the screen
                    await _statusLabel.TranslateTo(-10, 0, 250, Easing.CubicOut);
                    await _statusLabel.TranslateTo(0, 0, 250, Easing.CubicOut);

                    await pulseImageTask;
                    await Task.Delay(TimeSpan.FromMilliseconds(250));
                }
            });
        }

        Task PulseImage()
        {
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                //Image crouches down
                await _gitTrendsImage.ScaleTo(0.95, 100, Easing.CubicInOut);
                await Task.Delay(TimeSpan.FromMilliseconds(50));

                //Image jumps
                await _gitTrendsImage.ScaleTo(1.25, 250, Easing.CubicOut);

                //Image crashes back to the screen
                await _gitTrendsImage.ScaleTo(1, 500, Easing.BounceOut);
            });
        }

        Task ChangeLabelText(string text) => ChangeLabelText(new FormattedString
        {
            Spans =
            {
                new Span
                {
                    Text = text,
                    FontFamily = FontFamilyConstants.RobotoRegular
                }
            }
        });

        Task ChangeLabelText(string title, string body) => ChangeLabelText(new FormattedString
        {
            Spans =
            {
                new Span
                {
                    Text = title,
                    FontSize = 16,
                    FontFamily = FontFamilyConstants.RobotoBold
                },
                new Span
                {
                    Text = "\n" + body,
                    FontFamily = FontFamilyConstants.RobotoRegular
                }
            }
        });

        Task ChangeLabelText(FormattedString formattedString)
        {
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _statusLabel.FadeTo(0, 250, Easing.CubicOut);

                _statusLabel.Text = null;
                _statusLabel.FormattedText = formattedString;

                await _statusLabel.FadeTo(1, 250, Easing.CubicIn);
            });
        }

        async void HandleInitializationComplete(object sender, InitializationCompleteEventArgs e)
        {
            _animationCancellationToken?.Cancel();
            if (e.IsInitializationSuccessful)
            {
#if DEBUG
                await ChangeLabelText("Preview Mode", "Certain license warnings may appear");
                //Display Text
                await Task.Delay(TimeSpan.FromMilliseconds(500));
#else
                await ChangeLabelText("Let's go!");
#endif
                await NavigateToNextPage();
            }
            else
            {
                await ChangeLabelText("Initialization Failed", "\nEnsure Internet Connection Is Available and Update GitTrends to the Latest Version");

                AnalyticsService.Track("Initialization Failed");
            }

            Task NavigateToNextPage()
            {
                return MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    //Explode & Fade Everything
                    var explodeImageTask = Task.WhenAll(Content.ScaleTo(100, 250, Easing.CubicOut), Content.FadeTo(0, 250, Easing.CubicIn));
                    BackgroundColor = (Color)Application.Current.Resources[nameof(BaseTheme.PageBackgroundColor)];

                    using var scope = ContainerService.Container.BeginLifetimeScope();
                    var repositoryPage = scope.Resolve<RepositoryPage>();

                    await explodeImageTask;

                    Application.Current.MainPage = new BaseNavigationPage(repositoryPage);

                    if (FirstRunService.IsFirstRun)
                    {
                        //Yield the UI thread to allow MainPage to be set
                        await Task.Delay(TimeSpan.FromMilliseconds(250));

                        var onboardingCarouselPage = scope.Resolve<OnboardingCarouselPage>();
                        await repositoryPage.Navigation.PushModalAsync(onboardingCarouselPage);
                    }
                });
            }
        }
    }
}
