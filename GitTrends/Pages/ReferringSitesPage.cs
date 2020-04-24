using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using GitTrends.Views.Base;
using Xamarin.Essentials;
using Xamarin.Forms;
namespace GitTrends
{
    class ReferringSitesPage : BaseContentPage<ReferringSitesViewModel>
    {
        readonly StoreRatingRequestView _storeRatingRequestView = new StoreRatingRequestView();

namespace GitTrends
{
    class ReferringSitesPage : BaseContentPage<ReferringSitesViewModel>
    {
        readonly StoreRatingRequestView _storeRatingRequestView = new StoreRatingRequestView();
        readonly CancellationTokenSource _refreshViewCancelltionTokenSource = new CancellationTokenSource();

        readonly ReviewService _reviewService;
        readonly RefreshView _refreshView;
        readonly DeepLinkingService _deepLinkingService;

        public ReferringSitesPage(DeepLinkingService deepLinkingService,
                                    ReferringSitesViewModel referringSitesViewModel,
                                    Repository repository,
                                    AnalyticsService analyticsService,
                                    ReviewService reviewService) : base(referringSitesViewModel, analyticsService, PageTitles.ReferringSitesPage)
        {
            const int titleRowHeight = 50;
            const int titleTopMargin = 15;

            _deepLinkingService = deepLinkingService;
            _reviewService = reviewService;

            reviewService.ReviewCompleted += HandleReviewCompleted;

            var collectionView = new CollectionView
            {
                AutomationId = ReferringSitesPageAutomationIds.CollectionView,
                BackgroundColor = Color.Transparent,
                ItemTemplate = new ReferringSitesDataTemplate(),
                SelectionMode = SelectionMode.Single,
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
                //Set iOS Header to `new BoxView { HeightRequest = titleRowHeight + titleTopMargin }` following this bug fix: https://github.com/xamarin/Xamarin.Forms/issues/9879
                Header = Device.RuntimePlatform is Device.Android ? new BoxView { HeightRequest = 8 } : null,
                Footer = Device.RuntimePlatform is Device.Android ? new BoxView { HeightRequest = 8 } : null
                EmptyView = new ListEmptyState("EmptyReferringSitesList", 250, 250, "Your referring sites list is\nempty.")
            };
            collectionView.SelectionChanged += HandleCollectionViewSelectionChanged;
            collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(ReferringSitesViewModel.MobileReferringSitesList));

            _refreshView = new RefreshView
            {
                AutomationId = ReferringSitesPageAutomationIds.RefreshView,
                CommandParameter = (repository.OwnerLogin, repository.Name, _refreshViewCancelltionTokenSource.Token),
                Content = collectionView
            };
            _refreshView.SetDynamicResource(RefreshView.RefreshColorProperty, nameof(BaseTheme.PullToRefreshColor));
            _refreshView.SetBinding(RefreshView.CommandProperty, nameof(ReferringSitesViewModel.RefreshCommand));
            _refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(ReferringSitesViewModel.IsRefreshing));

            var relativeLayout = new RelativeLayout();

            //Add Title and Close Button to UIModalPresentationStyle.FormSheet 
            if (Device.RuntimePlatform is Device.iOS)
            {
                const int refreshViewTopPadding = titleRowHeight + 5;

                var closeButton = new Button
                {
                    Text = "Close",
                    FontFamily = FontFamilyConstants.RobotoRegular,
                    HeightRequest = titleRowHeight * 3 / 5,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center,
                    AutomationId = ReferringSitesPageAutomationIds.CloseButton,
                    Padding = new Thickness(5, 0),
                };
                closeButton.Clicked += HandleCloseButtonClicked;
                closeButton.SetDynamicResource(Button.TextColorProperty, nameof(BaseTheme.NavigationBarTextColor));
                closeButton.SetDynamicResource(Button.BorderColorProperty, nameof(BaseTheme.BorderButtonBorderColor));
                closeButton.SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.NavigationBarBackgroundColor));


                var titleRowBlurView = new BoxView { Opacity = 0.5 };
                titleRowBlurView.SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

                var titleLabel = new Label
                {
                    FontSize = 30,
                    Text = PageTitles.ReferringSitesPage,
                    FontFamily = FontFamilyConstants.RobotoMedium,
                };
                titleLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));

                closeButton.Margin = titleLabel.Margin = new Thickness(0, titleTopMargin, 0, 0);

                relativeLayout.Children.Add(_refreshView,
                                             Constraint.Constant(0),
                                             Constraint.Constant(refreshViewTopPadding), //Set to `0` following this bug fix: https://github.com/xamarin/Xamarin.Forms/issues/9879
                                             Constraint.RelativeToParent(parent => parent.Width),
                                             Constraint.RelativeToParent(parent => parent.Height - refreshViewTopPadding)); //Set to `parent => parent.Height` following this bug fix: https://github.com/xamarin/Xamarin.Forms/issues/9879

                relativeLayout.Children.Add(titleRowBlurView,
                                            Constraint.Constant(0),
                                            Constraint.Constant(0),
                                            Constraint.RelativeToParent(parent => parent.Width),
                                            Constraint.Constant(titleRowHeight));

                relativeLayout.Children.Add(titleLabel,
                                            Constraint.Constant(10),
                                            Constraint.Constant(0));

                relativeLayout.Children.Add(closeButton,
                                            Constraint.RelativeToParent(parent => parent.Width - GetWidth(parent, closeButton) - 10),
                                            Constraint.Constant(0),
                                            Constraint.RelativeToParent(parent => GetWidth(parent, closeButton)));
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
                                            Constraint.RelativeToParent(parent => parent.Height - GetHeight(parent, _storeRatingRequestView)),
                                            Constraint.RelativeToParent(parent => parent.Width));

            Content = relativeLayout;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_refreshView.Content is CollectionView collectionView && IsNullOrEmpty(collectionView.ItemsSource))
            {
                _refreshView.IsRefreshing = true;
                _reviewService.TryRequestReviewPrompt();
            }

            static bool IsNullOrEmpty(in IEnumerable? enumerable) => !enumerable?.GetEnumerator().MoveNext() ?? true;
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
                    { nameof(ReferringSiteModel.Referrer), referingSite.Referrer },
                    { nameof(ReferringSiteModel.ReferrerUri), referingSite.ReferrerUri.ToString() }
                });

                await _deepLinkingService.OpenBrowser(referingSite.ReferrerUri);
            }
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
