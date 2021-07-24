using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Sharpnado.MaterialFrame;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    class OrganizationsCarouselOverlay : Grid
    {
        readonly IMainThread _mainThread;

        public OrganizationsCarouselOverlay(IMainThread mainThread,
                                                IAnalyticsService analyticsService,
                                                MediaElementService mediaElementService)
        {
            _mainThread = mainThread;

            RowDefinitions = Rows.Define(
                (Row.CloseButton, Star),
                (Row.CarouselFrame, Stars(8)),
                (Row.BottomPadding, Star));

            ColumnDefinitions = Columns.Define(
                (Column.Left, Star),
                (Column.Center, Stars(8)),
                (Column.Right, Star));

            Children.Add(new BoxView { BackgroundColor = Color.White.MultiplyAlpha(0.25) }
                            .RowSpan(All<Row>()).ColumnSpan(All<Column>()));


            Children.Add(new CloseButton(() => Dismiss(true), analyticsService)
                            .Row(Row.CloseButton).Column(Column.Right));


            Children.Add(new OrganizationsCarouselFrame(analyticsService, mediaElementService)
                            .Row(Row.CarouselFrame).Column(Column.Center));

            Dismiss(false).SafeFireAndForget(ex => analyticsService.Report(ex));
        }

        enum Row { CloseButton, CarouselFrame, BottomPadding }
        enum Column { Left, Center, Right }

        public Task Reveal(bool shouldAnimate) => _mainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (shouldAnimate)
                await this.FadeTo(1);
            else
                Opacity = 1;

            InputTransparent = false;
        });

        public Task Dismiss(bool shouldAnimate) => _mainThread.InvokeOnMainThreadAsync(async () =>
        {
            InputTransparent = true;

            if (shouldAnimate)
                await this.FadeTo(0, 1000);
            else
                Opacity = 0;
        });

        class CloseButton : BounceButton
        {
            public CloseButton(Func<Task> dismissOverlay, IAnalyticsService analyticsService)
            {
                Text = "x";
                Command = new AsyncCommand(async () =>
                {
                    IsEnabled = false;

                    analyticsService.Track($"{nameof(OrganizationsCarouselOverlay)} Close Button Tapped");

                    // Make the button disappear before OrganizationsCarouselFrame
                    await Task.WhenAll(dismissOverlay(), this.FadeTo(0));

                    //Ensure the Button is visible and reenabled when it next appears
                    Opacity = 1;
                    IsEnabled = true;
                });

                BackgroundColor = Color.Transparent;

                FontSize = 24;
                FontFamily = FontFamilyConstants.RobotoBold;

                this.DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
            }
        }

        class OrganizationsCarouselFrame : MaterialFrame
        {
            const int _cornerRadius = 12;

            readonly IndicatorView _indicatorView;
            readonly IAnalyticsService _analyticsService;

            public OrganizationsCarouselFrame(IAnalyticsService analyticsService,
                                                MediaElementService mediaElementService)
            {
                _analyticsService = analyticsService;

                Padding = 0;

                CornerRadius = _cornerRadius;

                LightThemeBackgroundColor = GetBackgroundColor(0);

                Elevation = 8;

                Content = new EnableOrganizationsGrid
                {
                    IsClippedToBounds = true,

                    Children =
                    {
                        new OpacityOverlay()
                            .Row(EnableOrganizationsGrid.Row.Image),

                        new OrganizationsCarouselView(mediaElementService)
                            .Row(EnableOrganizationsGrid.Row.Image).RowSpan(All<EnableOrganizationsGrid.Row>())
                            .Invoke(view => view.PositionChanged += HandlePositionChanged)
                            .FillExpand(),

                        new EnableOrganizationsCarouselIndicatorView()
                            .Row(EnableOrganizationsGrid.Row.IndicatorView)
                            .Assign(out _indicatorView)
                    }
                };
            }

            static Color GetBackgroundColor(int position) => position % 2 is 0
                                                                ? Color.FromHex(BaseTheme.LightTealColorHex) // Even-numbered Pages are Teal
                                                                : Color.FromHex(BaseTheme.CoralColorHex); // Odd-numbered Pages are Coral

            void HandlePositionChanged(object sender, PositionChangedEventArgs e)
            {
                LightThemeBackgroundColor = GetBackgroundColor(e.CurrentPosition);
                _indicatorView.Position = e.CurrentPosition;

                _analyticsService.Track($"{nameof(OrganizationsCarouselView)} Page {e.CurrentPosition} Appeared");
            }

            class OpacityOverlay : PancakeView
            {
                public OpacityOverlay()
                {
                    SetBackgroundColor();
                    CornerRadius = new CornerRadius(_cornerRadius, _cornerRadius, 0, 0);

                    ThemeService.PreferenceChanged += HandlePreferenceChanged;
                }

                void HandlePreferenceChanged(object sender, PreferredTheme e) => SetBackgroundColor();

                void SetBackgroundColor() => BackgroundColor = Application.Current.Resources switch
                {
                    LightTheme => Color.White.MultiplyAlpha(0.25),
                    DarkTheme => Color.White.MultiplyAlpha(0.1),
                    _ => throw new NotSupportedException()
                };
            }

            class OrganizationsCarouselView : CarouselView
            {
                public OrganizationsCarouselView(MediaElementService mediaElementService)
                {
                    Loop = false;
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Never;

                    ItemsSource = new[]
                    {
                        new IncludeOrganizationsCarouselModel("Title 1", "Text 1", 0, "Business", null),
                        new IncludeOrganizationsCarouselModel("Title 2", "Text 2", 1, "Inspectocat", null),
                        new IncludeOrganizationsCarouselModel("Title 3", "Text 3", 2, null, mediaElementService.EnableOrganizationsUrl),
                    };

                    ItemTemplate = new EnableOrganizationsCarouselTemplateSelector();
                }
            }

            class EnableOrganizationsCarouselIndicatorView : IndicatorView
            {
                public EnableOrganizationsCarouselIndicatorView()
                {
                    Count = 3;
                    IsEnabled = false;
                    SelectedIndicatorColor = Color.White;
                    IndicatorColor = SelectedIndicatorColor.MultiplyAlpha(0.25);

                    WidthRequest = 112;
                    HeightRequest = 50;

                    IndicatorSize = 12;

                    AutomationId = SettingsPageAutomationIds.EnableOrangizationsPageIndicator;

                    this.Center().Margins(bottom: 12);
                }
            }
        }
    }
}
