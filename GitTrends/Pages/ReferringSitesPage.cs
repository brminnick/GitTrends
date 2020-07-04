using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using static Xamarin.Forms.Constraint;

namespace GitTrends
{
    partial class ReferringSitesPage : BaseContentPage<ReferringSitesViewModel>
    {
        const int _titleTopMargin = 10;

        readonly bool _isiOS = Device.RuntimePlatform is Device.iOS;
        readonly StoreRatingRequestView _storeRatingRequestView = new StoreRatingRequestView();
        readonly CancellationTokenSource _refreshViewCancelltionTokenSource = new CancellationTokenSource();

        readonly Repository _repository;
        readonly RefreshView _refreshView;
        readonly ThemeService _themeService;
        readonly ReviewService _reviewService;
        readonly DeepLinkingService _deepLinkingService;

        public ReferringSitesPage(DeepLinkingService deepLinkingService,
                                  ReferringSitesViewModel referringSitesViewModel,
                                  Repository repository,
                                  IAnalyticsService analyticsService,
                                  ThemeService themeService,
                                  ReviewService reviewService,
                                  IMainThread mainThread) : base(referringSitesViewModel, analyticsService, mainThread)
        {
            Title = PageTitles.ReferringSitesPage;

            _repository = repository;
            _themeService = themeService;
            _reviewService = reviewService;
            _deepLinkingService = deepLinkingService;

            reviewService.ReviewCompleted += HandleReviewCompleted;
            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

            var titleRowHeight = _isiOS ? 50 : 0;

            var collectionView = new ReferringSitesCollectionView()
                .Bind(IsVisibleProperty, nameof(ReferringSitesViewModel.IsEmptyDataViewEnabled))
                .Bind(EmptyDataView.TitleProperty, nameof(ReferringSitesViewModel.EmptyDataViewTitle))
                .Bind(EmptyDataView.DescriptionProperty, nameof(ReferringSitesViewModel.EmptyDataViewDescription))
                .Bind(CollectionView.ItemsSourceProperty, nameof(ReferringSitesViewModel.MobileReferringSitesList))
                .Invoke(collectionView => collectionView.SelectionChanged += HandleCollectionViewSelectionChanged);

            var closeButton = new CloseButton(titleRowHeight).Invoke(closeButton => closeButton.Clicked += HandleCloseButtonClicked);

            Content = new RelativeLayout()
                        .Add(new ReferringSitesRefreshView(collectionView, repository, _refreshViewCancelltionTokenSource.Token).Assign(out _refreshView)
                                .DynamicResource(RefreshView.RefreshColorProperty, nameof(BaseTheme.PullToRefreshColor))
                                .Bind(RefreshView.CommandProperty, nameof(ReferringSitesViewModel.RefreshCommand))
                                .Bind(RefreshView.IsRefreshingProperty, nameof(ReferringSitesViewModel.IsRefreshing)),
                            Constant(0), Constant(titleRowHeight), RelativeToParent(parent => parent.Width), RelativeToParent(parent => parent.Height - titleRowHeight))
                        .Add(_isiOS ? new TitleShadowView(themeService) : null,
                            Constant(0), Constant(0), RelativeToParent(parent => parent.Width), Constant(titleRowHeight))
                        .Add(_isiOS ? new TitleLabel() : null,
                            Constant(10), Constant(0))
                        .Add(_isiOS ? closeButton : null,
                            RelativeToParent(parent => parent.Width - closeButton.GetWidth(parent) - 10), Constant(0), RelativeToParent(parent => closeButton.GetWidth(parent)))
                        .Add(_storeRatingRequestView,
                            Constant(0), RelativeToParent(parent => parent.Height - _storeRatingRequestView.GetHeight(parent)), RelativeToParent(parent => parent.Width));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_refreshView?.Content is CollectionView collectionView
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

        class ReferringSitesRefreshView : RefreshView
        {
            public ReferringSitesRefreshView(in CollectionView collectionView, in Repository repository, in CancellationToken cancellationToken)
            {
                Content = collectionView;
                AutomationId = ReferringSitesPageAutomationIds.RefreshView;
                CommandParameter = (repository.OwnerLogin, repository.Name, repository.Url, cancellationToken);
            }
        }

        class ReferringSitesCollectionView : CollectionView
        {
            public ReferringSitesCollectionView()
            {
                AutomationId = ReferringSitesPageAutomationIds.CollectionView;
                BackgroundColor = Color.Transparent;
                ItemTemplate = new ReferringSitesDataTemplate();
                SelectionMode = SelectionMode.Single;
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);

                //Set iOS Header to `new BoxView { HeightRequest = titleRowHeight + titleTopMargin }` following this bug fix: https://github.com/xamarin/Xamarin.Forms/issues/9879
                Header = Device.RuntimePlatform is Device.iOS ? null : new BoxView { HeightRequest = ReferringSitesDataTemplate.BottomPadding };
                Footer = Device.RuntimePlatform is Device.iOS ? null : new BoxView { HeightRequest = ReferringSitesDataTemplate.TopPadding };
                EmptyView = new EmptyDataView("EmptyReferringSitesList", ReferringSitesPageAutomationIds.EmptyDataView);
            }
        }

        class TitleShadowView : BoxView
        {
            public TitleShadowView(in ThemeService themeService)
            {
                this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
                if (isLightTheme(themeService.Preference))
                {
                    On<iOS>()
                        .SetIsShadowEnabled(true)
                        .SetShadowColor(Color.Gray)
                        .SetShadowOffset(new Size(0, 1))
                        .SetShadowOpacity(0.5)
                        .SetShadowRadius(4);
                }

                static bool isLightTheme(in PreferredTheme preferredTheme) => preferredTheme is PreferredTheme.Light || preferredTheme is PreferredTheme.Default && Xamarin.Forms.Application.Current.RequestedTheme is OSAppTheme.Light;
            }
        }

        class TitleLabel : Label
        {
            public TitleLabel()
            {
                Text = PageTitles.ReferringSitesPage;
                this.Font(family: FontFamilyConstants.RobotoMedium, size: 30);
                this.DynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
                this.Center().Margins(top: _titleTopMargin).TextCenterVertical();
            }
        }

        class CloseButton : Button
        {
            public CloseButton(in int titleRowHeight)
            {
                Text = ReferringSitesPageConstants.CloseButtonText;
                AutomationId = ReferringSitesPageAutomationIds.CloseButton;
                this.Font(family: FontFamilyConstants.RobotoRegular);
                this.DynamicResources(
                    (TextColorProperty, nameof(BaseTheme.CloseButtonTextColor)),
                    (BackgroundColorProperty, nameof(BaseTheme.CloseButtonBackgroundColor)));
                this.End().CenterVertical().Margins(top: _titleTopMargin).Height(titleRowHeight * 3 / 5).Padding(5, 0);
            }
        }
    }
}
