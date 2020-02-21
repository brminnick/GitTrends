using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using GitTrends.Mobile.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class SplashScreenPage : ContentPage
    {
        readonly SyncFusionService _syncFusionService;
        readonly AnalyticsService _analyticsService;
        readonly IEnumerator<string> _statusMessageEnumerator;
        readonly Image _gitTrendsImage;
        readonly Label _statusLabel;

        public SplashScreenPage(SyncFusionService syncFusionService, AnalyticsService analyticsService)
        {
            _syncFusionService = syncFusionService;
            _analyticsService = analyticsService;

            IEnumerable<string> statusMessageList = new[] { "Initializing", "Connecting to servers", "Initializing", "Connecting to servers", "Initializing", "Connecting to servers", "Still working on it", "Let's try it like this", "Maybe this", "Another try", "Hmmm, it shouldn't take this long", "Are you sure the internet connection is good?" };
            _statusMessageEnumerator = statusMessageList.GetEnumerator();
            _statusMessageEnumerator.MoveNext();

            _gitTrendsImage = new Image
            {
                AutomationId = SplashScreenPageAutomationIds.GitTrendsImage,
                Source = "GitTrends",
                Opacity = 0,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Aspect = Aspect.AspectFit,
                WidthRequest = Device.RuntimePlatform is Device.iOS ? -1 : 250,
                HeightRequest = Device.RuntimePlatform is Device.iOS ? -1 : 190
            };

            _statusLabel = new Label
            {
                AutomationId = SplashScreenPageAutomationIds.StatusLabel,
                Text = _statusMessageEnumerator.Current,
                HorizontalTextAlignment = TextAlignment.Center,
                TranslationX = DeviceDisplay.MainDisplayInfo.Width / 2,
            };


            var relativeLayout = new RelativeLayout();

            relativeLayout.Children.Add(_statusLabel,
                Constraint.RelativeToParent(parent => parent.Width / 2 - getWidth(parent, _statusLabel) / 2),
                Constraint.RelativeToParent(parent => parent.Height - getHeight(parent, _statusLabel) - 50));

            relativeLayout.Children.Add(_gitTrendsImage,
                Constraint.RelativeToParent(parent => parent.Width / 2 - getWidth(parent, _gitTrendsImage) / 2),
                Constraint.RelativeToParent(parent => parent.Height / 2 - getHeight(parent, _gitTrendsImage) / 2));

            Content = relativeLayout;

            static double getWidth(RelativeLayout parent, View view) => view.Measure(parent.Width, parent.Height).Request.Width;
            static double getHeight(RelativeLayout parent, View view) => view.Measure(parent.Width, parent.Height).Request.Height;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            using var animationCancellationToken = new CancellationTokenSource();

            try
            {
                //Fade the Image Opacity to 1. Work around for https://github.com/xamarin/Xamarin.Forms/issues/8073
                var fadeImageTask = _gitTrendsImage.FadeTo(1, 1000, Easing.CubicIn);
                var pulseImageTask = PulseImage();

                //Slide status label into screen
                await _statusLabel.TranslateTo(-10, 0, 250, Easing.CubicOut);
                await _statusLabel.TranslateTo(0, 0, 250, Easing.CubicOut);

                //Wait for Image to reach an opacity of 1
                await Task.WhenAll(fadeImageTask, pulseImageTask);

#if DEBUG
                _syncFusionService.Initialize().SafeFireAndForget(ex => Debug.WriteLine(ex));
                await ChangeLabelText(new FormattedString
                {
                    Spans =
                    {
                        new Span
                        {
                            FontAttributes = FontAttributes.Bold,
                            Text = "Preview Mode"
                        },
                        new Span
                        {
                            Text = "\nCertain license warnings may appear"
                        }
                    }
                });

                //Display Text
                await Task.Delay(500);
#else

                Animate(animationCancellationToken.Token);

                await _syncFusionService.Initialize();
                animationCancellationToken.Cancel();

                await ChangeLabelText("Let's go!");
#endif

                //Explode & Fade Everything
                var explodeImageTask = Task.WhenAll(Content.ScaleTo(100, 250, Easing.CubicOut), Content.FadeTo(0, 250, Easing.CubicIn));
                BackgroundColor = (Color)Xamarin.Forms.Application.Current.Resources[nameof(BaseTheme.PageBackgroundColor)];

                await explodeImageTask;

                using var scope = ContainerService.Container.BeginLifetimeScope();
                Application.Current.MainPage = new BaseNavigationPage(scope.Resolve<RepositoryPage>());
            }
            catch (Exception e)
            {
                animationCancellationToken.Cancel();
                await ChangeLabelText(new FormattedString
                {
                    Spans =
                    {
                        new Span
                        {
                            FontAttributes = FontAttributes.Bold,
                            Text = "Initialization Failed"
                        },
                        new Span
                        {
                            Text = "\nPlease Ensure Internet Connection Is Available"
                        }
                    }
                });

                _analyticsService.Report(e);
                _analyticsService.Track("Initialization Failed");
            }
        }

        async void Animate(CancellationToken pulseCancellationToken)
        {
            while (!pulseCancellationToken.IsCancellationRequested)
            {
                var pulseImageTask = PulseImage();
                await Task.Delay(400);

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
                await Task.Delay(250);
            }
        }

        async Task PulseImage()
        {
            //Image crouches down
            await _gitTrendsImage.ScaleTo(0.95, 100, Easing.CubicInOut);
            await Task.Delay(50);

            //Image jumps
            await _gitTrendsImage.ScaleTo(1.25, 250, Easing.CubicOut);

            //Image crashes back to the screen
            await _gitTrendsImage.ScaleTo(1, 500, Easing.BounceOut);
        }

        async Task ChangeLabelText(FormattedString formattedString)
        {
            await _statusLabel.FadeTo(0, 250, Easing.CubicOut);

            _statusLabel.Text = null;
            _statusLabel.FormattedText = formattedString;

            await _statusLabel.FadeTo(1, 250, Easing.CubicIn);
        }

        Task ChangeLabelText(string text) => ChangeLabelText(new FormattedString { Spans = { new Span { Text = text } } });
    }
}
