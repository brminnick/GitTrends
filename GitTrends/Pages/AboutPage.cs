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
        public AboutPage(IMainThread mainThread,
                            AboutViewModel aboutViewModel,
                            IAnalyticsService analyticsService) : base(aboutViewModel, analyticsService, mainThread)
        {
            Title = AboutPageConstants.About;

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Children =
                    {
                        new CollectionView
                        {
                            ItemTemplate = new ContributorDataTemplate(),
                            ItemsSource = ViewModel.GitTrendsContributors,
                            ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical)
                        }
                    }
                }
            };

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

                    new Label().TextCenterHorizontal().TextTop().Font(12, family: FontFamilyConstants.RobotoRegular)
                        .Row(Row.Login)
                        .Bind(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime)
                }
            };

            enum Row { Avatar, Login }
        }
    }
}
