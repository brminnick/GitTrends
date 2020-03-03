using System;
using System.Collections;
using System.Linq;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    class ReferringSitesPage : BaseContentPage<ReferringSitesViewModel>
    {
        readonly RefreshView _refreshView;

        public ReferringSitesPage(ReferringSitesViewModel referringSitesViewModel,
                                    Repository repository,
                                    AnalyticsService analyticsService) : base(PageTitles.ReferringSitesPage, referringSitesViewModel, analyticsService)
        {
            const int titleRowHeight = 50;
            const int titleTopMargin = 15;

            var collectionView = new CollectionView
            {
                AutomationId = ReferringSitesPageAutomationIds.CollectionView,
                ItemTemplate = new ReferringSitesDataTemplateSelector(),
                SelectionMode = SelectionMode.Single
            };
            collectionView.SelectionChanged += HandleCollectionViewSelectionChanged;
            collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(ReferringSitesViewModel.ReferringSitesCollection));

            _refreshView = new RefreshView
            {
                AutomationId = ReferringSitesPageAutomationIds.RefreshView,
                CommandParameter = (repository.OwnerLogin, repository.Name),
                Content = collectionView
            };
            _refreshView.SetDynamicResource(RefreshView.RefreshColorProperty, nameof(BaseTheme.RefreshControlColor));
            _refreshView.SetBinding(RefreshView.CommandProperty, nameof(ReferringSitesViewModel.RefreshCommand));
            _refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(ReferringSitesViewModel.IsRefreshing));

            //Add Title and Back Button to UIModalPresentationStyle.FormSheet 
            if (Device.RuntimePlatform is Device.iOS)
            {
                var closeButton = new Button
                {
                    AutomationId = ReferringSitesPageAutomationIds.CloseButton,
                    Text = "Close",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center,
                    HeightRequest = titleRowHeight * 3 / 5,
                    Padding = new Thickness(5, 0)
                };
                closeButton.Clicked += HandleCloseButtonClicked;
                closeButton.SetDynamicResource(Button.TextColorProperty, nameof(BaseTheme.NavigationBarTextColor));
                closeButton.SetDynamicResource(Button.BorderColorProperty, nameof(BaseTheme.TrendsChartSettingsBorderColor));
                closeButton.SetDynamicResource(Button.BackgroundColorProperty, nameof(BaseTheme.NavigationBarBackgroundColor));


                var titleRowBlurView = new BoxView { Opacity = 0.5 };
                titleRowBlurView.SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

                collectionView.Header = new BoxView { HeightRequest = titleRowHeight + titleTopMargin };

                var titleLabel = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    Text = PageTitles.ReferringSitesPage,
                    FontSize = 30
                };
                titleLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));

                closeButton.Margin = titleLabel.Margin = new Thickness(0, titleTopMargin, 0, 0);

                var activityIndicator = new ActivityIndicator
                {
                    AutomationId = ReferringSitesPageAutomationIds.ActivityIndicator,
                };
                activityIndicator.SetDynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.RefreshControlColor));
                activityIndicator.SetBinding(IsVisibleProperty, nameof(ReferringSitesViewModel.IsActivityIndicatorVisible));
                activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ReferringSitesViewModel.IsActivityIndicatorVisible));

                var relativeLayout = new RelativeLayout();

                relativeLayout.Children.Add(_refreshView,
                                             Constraint.Constant(0),
                                             Constraint.Constant(0),
                                             Constraint.RelativeToParent(parent => parent.Width),
                                             Constraint.RelativeToParent(parent => parent.Height));

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

                relativeLayout.Children.Add(activityIndicator,
                                            Constraint.RelativeToParent(parent => parent.Width / 2 - GetWidth(parent, activityIndicator) / 2),
                                            Constraint.RelativeToParent(parent => parent.Height / 2 - GetHeight(parent, activityIndicator) / 2));

                Content = relativeLayout;
            }
            else
            {
                Content = _refreshView;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Disappearing += HandleDisappearing;

            if (_refreshView.Content is CollectionView collectionView && IsNullOrEmpty(collectionView.ItemsSource))
                _refreshView.IsRefreshing = true;

            static bool IsNullOrEmpty(in IEnumerable? enumerable) => !enumerable?.GetEnumerator().MoveNext() ?? true;
        }

        async void HandleCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e?.CurrentSelection.FirstOrDefault() is ReferringSiteModel referingSite
                && referingSite.IsReferrerUriValid
                && referingSite.ReferrerUri != null)
            {
                Disappearing -= HandleDisappearing;

                await OpenBrowser(referingSite.ReferrerUri);
            }
        }

        //Workaround for https://github.com/xamarin/Xamarin.Forms/issues/7878
        async void HandleDisappearing(object sender, EventArgs e)
        {
            if (Navigation.ModalStack.Any())
                await Navigation.PopModalAsync();
        }

        async void HandleCloseButtonClicked(object sender, EventArgs e) => await Navigation.PopModalAsync();
    }
}
