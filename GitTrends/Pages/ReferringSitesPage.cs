using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    class ReferringSitesPage : BaseContentPage<ReferringSitesViewModel>
    {
        readonly StoreRatingRequestView _storeRatingRequestView = new StoreRatingRequestView();
        readonly CancellationTokenSource _refreshViewCancelltionTokenSource = new CancellationTokenSource();

        readonly RefreshView _refreshView;
        readonly ReviewService _reviewService;
        readonly DeepLinkingService _deepLinkingService;

        public ReferringSitesPage(DeepLinkingService deepLinkingService,
                                    ReferringSitesViewModel referringSitesViewModel,
                                    Repository repository,
                                    IAnalyticsService analyticsService,
                                    ThemeService themeService,
                                    ReviewService reviewService,
                                    IMainThread mainThread) : base(referringSitesViewModel, analyticsService, mainThread, PageTitles.ReferringSitesPage)
        {
            _reviewService = reviewService;
            _deepLinkingService = deepLinkingService;

            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;
            reviewService.ReviewCompleted += HandleReviewCompleted;

            var collectionView = new CollectionView
            {
                AutomationId = ReferringSitesPageAutomationIds.CollectionView,
                BackgroundColor = Color.Transparent,
                ItemTemplate = new ReferringSitesDataTemplate(),
                SelectionMode = SelectionMode.Single,
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
                //Set iOS Header to `new BoxView { HeightRequest = titleRowHeight + titleTopMargin }` following this bug fix: https://github.com/xamarin/Xamarin.Forms/issues/9879
                Header = Device.RuntimePlatform is Device.Android ? new BoxView { HeightRequest = ReferringSitesDataTemplate.BottomPadding } : null,
                Footer = Device.RuntimePlatform is Device.Android ? new BoxView { HeightRequest = ReferringSitesDataTemplate.TopPadding } : null,
                EmptyView = new EmptyDataView("EmptyReferringSitesList", ReferringSitesPageAutomationIds.EmptyDataView)
                                .Bind(IsVisibleProperty, nameof(ReferringSitesViewModel.IsEmptyDataViewEnabled))
                                .Bind(EmptyDataView.TitleProperty, nameof(ReferringSitesViewModel.EmptyDataViewTitle))
                                .Bind(EmptyDataView.DescriptionProperty, nameof(ReferringSitesViewModel.EmptyDataViewDescription))
            };
            collectionView.SelectionChanged += HandleCollectionViewSelectionChanged;
            collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(ReferringSitesViewModel.MobileReferringSitesList));

            _refreshView = new RefreshView
            {
                AutomationId = ReferringSitesPageAutomationIds.RefreshView,
                CommandParameter = (repository.OwnerLogin, repository.Name, repository.Url, _refreshViewCancelltionTokenSource.Token),
                Content = collectionView
            };
            _refreshView.SetDynamicResource(RefreshView.RefreshColorProperty, nameof(BaseTheme.PullToRefreshColor));
            _refreshView.SetBinding(RefreshView.CommandProperty, nameof(ReferringSitesViewModel.RefreshCommand));
            _refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(ReferringSitesViewModel.IsRefreshing));

            var relativeLayout = new RelativeLayout();

            //Add Title and Close Button to UIModalPresentationStyle.FormSheet 
            if (Device.RuntimePlatform is Device.iOS)
            {
                const int titleTopMargin = 10;
                const int titleRowHeight = 50;

                var closeButton = new Button
                {
                    Text = "Close",
                    FontFamily = FontFamilyConstants.RobotoRegular,
                    HeightRequest = titleRowHeight * 3 / 5,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center,
                    AutomationId = ReferringSitesPageAutomationIds.CloseButton,
                    Padding = new Thickness(5, 0)
                };
                closeButton.Clicked += HandleCloseButtonClicked;
                closeButton.SetDynamicResource(Button.TextColorProperty, nameof(BaseTheme.CloseButtonTextColor));
                closeButton.SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CloseButtonBackgroundColor));

                var titleLabel = new Label
                {
                    FontSize = 30,
                    Text = PageTitles.ReferringSitesPage,
                    FontFamily = FontFamilyConstants.RobotoMedium,
                }.Center().TextCenterVertical();
                titleLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));

                closeButton.Margin = titleLabel.Margin = new Thickness(0, titleTopMargin, 0, 0);

                var titleShadow = new BoxView();
                titleShadow.SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));

                if (isLightTheme(themeService.Preference))
                {
                    titleShadow.On<iOS>().SetIsShadowEnabled(true)
                                           .SetShadowColor(Color.Gray)
                                           .SetShadowOffset(new Size(0, 1))
                                           .SetShadowOpacity(0.5)
                                           .SetShadowRadius(4);
                }


                relativeLayout.Children.Add(_refreshView,
                                             Constraint.Constant(0),
                                             Constraint.Constant(titleRowHeight), //Set to `0` following this bug fix: https://github.com/xamarin/Xamarin.Forms/issues/9879
                                             Constraint.RelativeToParent(parent => parent.Width),
                                             Constraint.RelativeToParent(parent => parent.Height - titleRowHeight)); //Set to `parent => parent.Height` following this bug fix: https://github.com/xamarin/Xamarin.Forms/issues/9879

                relativeLayout.Children.Add(titleShadow,
                                            Constraint.Constant(0),
                                            Constraint.Constant(0),
                                            Constraint.RelativeToParent(parent => parent.Width),
                                            Constraint.Constant(titleRowHeight));

                relativeLayout.Children.Add(titleLabel,
                                            Constraint.Constant(10),
                                            Constraint.Constant(0));

                relativeLayout.Children.Add(closeButton,
                                            Constraint.RelativeToParent(parent => parent.Width - closeButton.GetWidth(parent) - 10),
                                            Constraint.Constant(0),
                                            Constraint.RelativeToParent(parent => closeButton.GetWidth(parent)));
            }
            else
            {
                relativeLayout.Children.Add(_refreshView,
                                            Constraint.Constant(0),
                                            Constraint.Constant(0),
                                            Constraint.RelativeToParent(parent => parent.Width),
                                            Constraint.RelativeToParent(parent => parent.Height));
            }

            relativeLayout.Children.Add(_storeRatingRequestView,
                                            Constraint.Constant(0),
                                            Constraint.RelativeToParent(parent => parent.Height - _storeRatingRequestView.GetHeight(parent)),
                                            Constraint.RelativeToParent(parent => parent.Width));

            Content = relativeLayout;

            static bool isLightTheme(PreferredTheme preferredTheme) => preferredTheme is PreferredTheme.Light || preferredTheme is PreferredTheme.Default && Xamarin.Forms.Application.Current.RequestedTheme is OSAppTheme.Light;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_refreshView.Content is CollectionView collectionView
                && collectionView.ItemsSource.IsNullOrEmpty())
            {
                _refreshView.IsRefreshing = true;
                _reviewService.TryRequestReviewPrompt();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _refreshViewCancelltionTokenSource.Cancel();
            _storeRatingRequestView.IsVisible = false;
        }

        async void HandleCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e?.CurrentSelection.FirstOrDefault() is ReferringSiteModel referingSite
                && referingSite.IsReferrerUriValid
                && referingSite.ReferrerUri != null)
            {
                AnalyticsService.Track("Referring Site Tapped", new Dictionary<string, string>
                {
                    { nameof(ReferringSiteModel) + nameof(ReferringSiteModel.Referrer), referingSite.Referrer },
                    { nameof(ReferringSiteModel) + nameof(ReferringSiteModel.ReferrerUri), referingSite.ReferrerUri.ToString() }
                });

                await _deepLinkingService.OpenBrowser(referingSite.ReferrerUri);
            }
        }

        void HandlePullToRefreshFailed(object sender, PullToRefreshFailedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Xamarin.Forms.Application.Current.MainPage.Navigation.ModalStack.LastOrDefault() is ReferringSitesPage
                    || Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Last() is ReferringSitesPage)
                {
                    if (e.Accept is null)
                    {
                        await DisplayAlert(e.Title, e.Message, e.Cancel);
                    }
                    else
                    {
                        var isAccepted = await DisplayAlert(e.Title, e.Message, e.Accept, e.Cancel);
                        if (isAccepted)
                            await _deepLinkingService.OpenBrowser(GitHubConstants.GitHubRateLimitingDocs);
                    }
                }
            });
        }

        void HandleReviewCompleted(object sender, ReviewRequest e) => MainThread.BeginInvokeOnMainThread(async () =>
        {
            const int animationDuration = 300;

            await Task.WhenAll(_storeRatingRequestView.TranslateTo(0, _storeRatingRequestView.Height, animationDuration),
                                _storeRatingRequestView.ScaleTo(0, animationDuration));

            _storeRatingRequestView.IsVisible = false;
            _storeRatingRequestView.Scale = 1;
            _storeRatingRequestView.TranslationY = 0;
        });

        async void HandleCloseButtonClicked(object sender, EventArgs e) => await Navigation.PopModalAsync();
    }
}
