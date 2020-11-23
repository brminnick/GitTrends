using System.Linq;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class AboutPage : BaseContentPage<AboutViewModel>
    {
        readonly DeepLinkingService _deepLinkingService;

        public AboutPage(IMainThread mainThread,
                            AboutViewModel aboutViewModel,
                            IAnalyticsService analyticsService,
                            DeepLinkingService deepLinkingService) : base(aboutViewModel, analyticsService, mainThread)
        {
            _deepLinkingService = deepLinkingService;

            Title = AboutPageConstants.About;

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Children =
                    {
                        new CollectionView
                        {
                            SelectionMode = SelectionMode.Single,
                            ItemTemplate = new ContributorDataTemplate(),
                            ItemsSource = ViewModel.GitTrendsContributors,
                            ItemsLayout = new GridItemsLayout(XamarinFormsService.IsSmallScreen ? 4: 5, ItemsLayoutOrientation.Vertical)
                        }.Invoke(collectionView => collectionView.SelectionChanged += HandleContributorSelectionChanged)
                    }
                }
            };
        }

        async void HandleContributorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e.CurrentSelection.FirstOrDefault() is Contributor contributor)
            {
                AnalyticsService.Track("Contributor Tapped", nameof(Contributor.Login), contributor.Login);

                await _deepLinkingService.OpenBrowser(contributor.GitHubUrl);
            }
        }

        class ContributorDataTemplate : DataTemplate
        {
            const int _circleDiameter = 50;

            public ContributorDataTemplate() : base(CreateContributorDataTemplate)
            {

            }

            static Grid CreateContributorDataTemplate() => new Grid
            {
                RowSpacing = 4,

                RowDefinitions = Rows.Define(
                    (Row.Avatar, AbsoluteGridLength(_circleDiameter)),
                    (Row.Login, AbsoluteGridLength(25))),

                Children =
                {
                    new AvatarImage(_circleDiameter)
                        .Row(Row.Avatar)
                        .Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime)
                        .DynamicResource(CircleImage.BorderColorProperty, nameof(BaseTheme.SeparatorColor)),

                    new Label { LineBreakMode = LineBreakMode.TailTruncation }.TextCenterHorizontal().TextTop().Font(12, family: FontFamilyConstants.RobotoRegular)
                        .Row(Row.Login)
                        .Bind(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime)
                        .DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
                }
            };

            enum Row { Avatar, Login }
        }
    }
}
