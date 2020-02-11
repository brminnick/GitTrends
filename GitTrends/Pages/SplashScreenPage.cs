using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class SplashScreenPage : ContentPage
    {
        readonly SyncFusionService _syncFusionService;
        readonly Image _gitTrendsImage;
        readonly Label _statusLabel;

        public SplashScreenPage(SyncFusionService syncFusionService)
        {
            _syncFusionService = syncFusionService;

            _gitTrendsImage = new Image
            {
                Source = "GitTrends",
                Opacity = 0,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
            };

            _statusLabel = new Label
            {
                Text = "Initializing",
                HorizontalTextAlignment = TextAlignment.Center,
                TranslationX = DeviceDisplay.MainDisplayInfo.Width / 2,
            };

            if (Device.RuntimePlatform is Device.Android)
            {
                _gitTrendsImage.WidthRequest = 250;
                _gitTrendsImage.HeightRequest = 190;
            }

            var relativeLayout = new RelativeLayout();
            relativeLayout.Children.Add(_gitTrendsImage,
                Constraint.RelativeToParent(parent => parent.Width / 2 - getWidth(parent, _gitTrendsImage) / 2),
                Constraint.RelativeToParent(parent => parent.Height / 2 - getHeight(parent, _gitTrendsImage) / 2));
            relativeLayout.Children.Add(_statusLabel,
                Constraint.RelativeToParent(parent => parent.Width / 2 - getWidth(parent, _statusLabel) / 2),
                Constraint.RelativeToView(_gitTrendsImage, (parent, view) => view.Y + getHeight(parent, _gitTrendsImage) + 20));

            Content = relativeLayout;

            static double getWidth(RelativeLayout parent, View view) => view?.Measure(parent.Width, parent.Height).Request.Width ?? -1;
            static double getHeight(RelativeLayout parent, View view) => view?.Measure(parent.Width, parent.Height).Request.Height ?? -1;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            using var pulseCancellationTokenSource = new CancellationTokenSource();

            try
            {
                //Fade the Image Opacity to 1. Work around for https://github.com/xamarin/Xamarin.Forms/issues/8073
                var fadeImageTask = _gitTrendsImage.FadeTo(1, 1000, Easing.CubicIn);

                //Slide status label into screen
                await _statusLabel.TranslateTo(-10, 0, 250, Easing.CubicOut);
                await _statusLabel.TranslateTo(0, 0, 250, Easing.CubicOut);

                //Wait for Image to reach an opacity of 1
                await fadeImageTask;

#if DEBUG
                _syncFusionService.Initialize().SafeFireAndForget();
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
                            Text = "\nLaunching app without Syncfusion License"
                        }
                    }
                });

                //Display Text for 1 second
                await Task.Delay(500);
#else

                PulseImage(pulseCancellationTokenSource.Token);
                SlideStatusLabel(pulseCancellationTokenSource.Token);

                await _syncFusionService.Initialize();
                pulseCancellationTokenSource.Cancel();

                await ChangeLabelText(new FormattedString { Spans = { new Span { Text = "Let's go!" } } });
#endif

                //Explode Content
                var explodeImageTask = Task.WhenAll(Content.ScaleTo(100, 250, Easing.CubicOut), Content.FadeTo(0, 250, Easing.CubicIn));
                BackgroundColor = (Color)Application.Current.Resources[nameof(BaseTheme.PageBackgroundColor)];

                await explodeImageTask;

                using var scope = ContainerService.Container.BeginLifetimeScope();
                Application.Current.MainPage = new BaseNavigationPage(scope.Resolve<RepositoryPage>());
            }
            catch
            {
                pulseCancellationTokenSource.Cancel();
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
            }
        }

        async void PulseImage(CancellationToken pulseCancellationToken)
        {
            while (!pulseCancellationToken.IsCancellationRequested)
            {
                //Image 'crouches down' peparing to jump
                await _gitTrendsImage.ScaleTo(0.95, 100, Easing.CubicInOut);
                await Task.Delay(50);

                //Image jumps
                await _gitTrendsImage.ScaleTo(1.25, 250, Easing.CubicOut);

                //Image crashes back to the screen
                await _gitTrendsImage.ScaleTo(1, 500, Easing.BounceOut);

                await Task.Delay(250);
            }
        }

        async void SlideStatusLabel(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //Label stays centered on the screen for 2 seconds
                await Task.Delay(2000);

                //Label leaves the screen
                await _statusLabel.TranslateTo(10, 0, 100, Easing.CubicInOut);
                await _statusLabel.TranslateTo(-DeviceDisplay.MainDisplayInfo.Width / 2, 0, 250, Easing.CubicIn);

                //Move the label to the other side of the screen
                _statusLabel.TranslationX = DeviceDisplay.MainDisplayInfo.Width / 2;
                await Task.Delay(100);

                //Label reappears on the screen
                await _statusLabel.TranslateTo(-10, 0, 250, Easing.CubicOut);
                await _statusLabel.TranslateTo(0, 0, 250, Easing.CubicOut);
            }
        }

        async Task ChangeLabelText(FormattedString formattedString)
        {
            await _statusLabel.FadeTo(0, 250, Easing.CubicOut);

            _statusLabel.Text = null;
            _statusLabel.FormattedText = formattedString;

            await _statusLabel.FadeTo(1, 250, Easing.CubicIn);
        }
    }
}
