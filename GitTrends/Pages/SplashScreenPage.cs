using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class SplashScreenPage : BaseContentPage<SplashScreenViewModel>
    {
        readonly IEnumerator<string> _statusMessageEnumerator = new List<string>
        {
            SplashScreenPageConstants.Initializing,
            SplashScreenPageConstants.ConnectingToServers,
            SplashScreenPageConstants.Initializing,
            SplashScreenPageConstants.ConnectingToServers,
            SplashScreenPageConstants.Initializing,
            SplashScreenPageConstants.ConnectingToServers,
            SplashScreenPageConstants.StillWorkingOnIt,
            SplashScreenPageConstants.LetsTryItLikeThis,
            SplashScreenPageConstants.MaybeThis,
            SplashScreenPageConstants.AnotherTry,
            SplashScreenPageConstants.ItShouldntTakeThisLong,
            SplashScreenPageConstants.AreYouSureInternetConnectionIsGood
        }.GetEnumerator();

        readonly Label _loadingLabel;
        readonly Image _gitTrendsImage;
        readonly FirstRunService _firstRunService;

        CancellationTokenSource? _animationCancellationToken;

        public SplashScreenPage(IMainThread mainThread,
                                    FirstRunService firstRunService,
                                    IAnalyticsService analyticsService,
                                    SplashScreenViewModel splashScreenViewModel)
            : base(splashScreenViewModel, analyticsService, mainThread)
        {
            //Remove BaseContentPageBackground
            RemoveDynamicResource(BackgroundColorProperty);
            this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.GitTrendsImageBackgroundColor));

            _firstRunService = firstRunService;

            SplashScreenViewModel.InitializationCompleted += HandleInitializationCompleted;

            _statusMessageEnumerator.MoveNext();

            Content = new Grid
            {
                RowDefinitions = Rows.Define(
                    (Row.Image, Star),
                    (Row.Text, Auto),
                    (Row.BottomPadding, AbsoluteGridLength(50))),

                Children =
                {
                    new LoadingLabel().Center().Assign(out _loadingLabel)
                        .Row(Row.Text),

                    new GitTrendsImage().Center().Assign(out _gitTrendsImage)
                        .RowSpan(All<Row>()),                    
                }
            };
        }

        enum Row { Image, Text, BottomPadding }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ChangeLabelText(_statusMessageEnumerator.Current);

            _animationCancellationToken = new CancellationTokenSource();

            //Fade the Image Opacity to 1. Work around for https://github.com/xamarin/Xamarin.Forms/issues/8073
            var fadeImageTask = _gitTrendsImage.FadeTo(1, 1000, Easing.CubicIn);
            var pulseImageTask = PulseImage();

            //Slide status label into screen
            await _loadingLabel.TranslateTo(-10, 0, 250, Easing.CubicOut);
            await _loadingLabel.TranslateTo(0, 0, 250, Easing.CubicOut);

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
                    await _loadingLabel.TranslateTo(10, 0, 100, Easing.CubicInOut);
                    await _loadingLabel.TranslateTo(-DeviceDisplay.MainDisplayInfo.Width / 2, 0, 250, Easing.CubicIn);

                    //Move the label to the other side of the screen
                    _loadingLabel.TranslationX = DeviceDisplay.MainDisplayInfo.Width / 2;

                    //Update Status Label Text
                    if (!_statusMessageEnumerator.MoveNext())
                    {
                        _statusMessageEnumerator.Reset();
                        _statusMessageEnumerator.MoveNext();
                    }
                    await ChangeLabelText(_statusMessageEnumerator.Current);

                    //Label reappears on the screen
                    await _loadingLabel.TranslateTo(-10, 0, 250, Easing.CubicOut);
                    await _loadingLabel.TranslateTo(0, 0, 250, Easing.CubicOut);

                    await pulseImageTask;
                    await Task.Delay(TimeSpan.FromMilliseconds(250));
                }
            });
        }

        Task PulseImage() => MainThread.InvokeOnMainThreadAsync(async () =>
        {
            //Image crouches down
            await _gitTrendsImage.ScaleTo(0.95, 100, Easing.CubicInOut);
            await Task.Delay(TimeSpan.FromMilliseconds(50));

            //Image jumps
            await _gitTrendsImage.ScaleTo(1.25, 250, Easing.CubicOut);

            //Image crashes back to the screen
            await _gitTrendsImage.ScaleTo(1, 500, Easing.BounceOut);
        });

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

        Task ChangeLabelText(FormattedString formattedString) => MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await _loadingLabel.FadeTo(0, 250, Easing.CubicOut);

            _loadingLabel.Text = null;
            _loadingLabel.FormattedText = formattedString;

            await _loadingLabel.FadeTo(1, 250, Easing.CubicIn);
        });

        async void HandleInitializationCompleted(object sender, InitializationCompleteEventArgs e)
        {
            _animationCancellationToken?.Cancel();
            if (e.IsInitializationSuccessful)
            {
#if DEBUG
                await ChangeLabelText(SplashScreenPageConstants.PreviewMode, SplashScreenPageConstants.WarningsMayAppear);
                //Display Text
                await Task.Delay(TimeSpan.FromMilliseconds(500));
#else
                await ChangeLabelText("Let's go!");
#endif
                await NavigateToNextPage();
            }
            else
            {
                await ChangeLabelText(SplashScreenPageConstants.InitializationFailed, $"\n{SplashScreenPageConstants.EnsureInternetConnectionAndLatestVersion}");

                AnalyticsService.Track("Initialization Failed");
            }

            Task NavigateToNextPage()
            {
                return MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    //Explode & Fade Everything
                    var explodeImageTask = Task.WhenAll(Content.ScaleTo(100, 250, Easing.CubicOut), Content.FadeTo(0, 250, Easing.CubicIn));
                    BackgroundColor = (Color)Application.Current.Resources[nameof(BaseTheme.PageBackgroundColor)];

                    var repositoryPage = ContainerService.Container.Resolve<RepositoryPage>();

                    if (_firstRunService.IsFirstRun)
                        repositoryPage.Appearing += HandleRepositoryPageAppearing;

                    await explodeImageTask;

                    Application.Current.MainPage = new BaseNavigationPage(repositoryPage);

                    async void HandleRepositoryPageAppearing(object sender, EventArgs e)
                    {
                        repositoryPage.Appearing -= HandleRepositoryPageAppearing;

                        //Yield the UI thread to allow MainPage to be set
                        await Task.Delay(TimeSpan.FromMilliseconds(500));

                        var onboardingCarouselPage = ContainerService.Container.Resolve<OnboardingCarouselPage>();
                        await repositoryPage.Navigation.PushModalAsync(onboardingCarouselPage);
                    }
                });
            }
        }

        class GitTrendsImage : Image
        {
            public GitTrendsImage()
            {
                Opacity = 0;
                Aspect = Aspect.AspectFit;
                AutomationId = SplashScreenPageAutomationIds.GitTrendsImage;

                this.CenterExpand().DynamicResource(SourceProperty, nameof(BaseTheme.GitTrendsImageSource));
            }
        }

        class LoadingLabel : Label
        {
            public LoadingLabel()
            {
                //Begin with Label off of the screen
                TranslationX = DeviceDisplay.MainDisplayInfo.Width / 2;

                Margin = new Thickness(10, 0);
                HorizontalTextAlignment = TextAlignment.Center;
                AutomationId = SplashScreenPageAutomationIds.StatusLabel;
                this.DynamicResource(TextColorProperty, nameof(BaseTheme.SplashScreenStatusColor));
            }
        }
    }
}
