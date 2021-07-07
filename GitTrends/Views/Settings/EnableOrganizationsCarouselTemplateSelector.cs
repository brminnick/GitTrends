using System.Threading;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace GitTrends
{
    class EnableOrganizationsCarouselTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => new EnableOrganizationsCarouselTemplate((IncludeOrganizationsCarouselModel)item);

        class EnableOrganizationsCarouselTemplate : DataTemplate
        {
            public EnableOrganizationsCarouselTemplate(IncludeOrganizationsCarouselModel includeOrganizationsModel) : base(() => CreateItemsGrid(includeOrganizationsModel))
            {

            }

            static EnableOrganizationsGrid CreateItemsGrid(IncludeOrganizationsCarouselModel includeOrganizationsModel) => new()
            {
                RowSpacing = 0, // Must be zero to match OrganizationsCarouselFrame's Grid

                Children =
                {
                    includeOrganizationsModel.Url is not null
                        ? new VideoPlayerView()
                            .Row(EnableOrganizationsGrid.Row.Image)
                        : new Image { Source = includeOrganizationsModel.ImageSource, Aspect = Aspect.AspectFit }.Margin(24, 12)
                            .Row(EnableOrganizationsGrid.Row.Image),

                    new TitleLabel(includeOrganizationsModel.Title)
                        .Row(EnableOrganizationsGrid.Row.Title),

                    new DescriptionLabel(includeOrganizationsModel.Text)
                        .Row(EnableOrganizationsGrid.Row.Description),

                    new GitHubButton(SettingsPageAutomationIds.GitHubButton, SettingsPageConstants.ManageOrganizations)
                        .Row(EnableOrganizationsGrid.Row.GitHubButton)
                        .Bind(GitHubButton.CommandProperty, nameof(SettingsViewModel.ManageOrganizationsButtonCommand), source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(SettingsViewModel)))
                        .Bind<GitHubButton, int, bool>(GitHubButton.IsVisibleProperty, nameof(CarouselView.Position), convert: position => position is 2, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(CarouselView))),
#if AppStore
#error: GitHubButton appears before CarouselView has finished swiping (ie The user can see it appear on the Inspectocat page)
#endif
                    new BoxView()
                        .Row(EnableOrganizationsGrid.Row.IndicatorView)
                }
            };

            class TitleLabel : Label
            {
                public TitleLabel(in string text)
                {
                    Text = text;
                    FontSize = 34;
                    TextColor = Color.White;
                    LineHeight = 1.12;
                    FontFamily = FontFamilyConstants.RobotoBold;
                    AutomationId = SettingsPageAutomationIds.EnableOrangizationsCarouselTitle;

                    Padding = new Thickness(24, 5);
                }
            }

            class DescriptionLabel : Label
            {
                public DescriptionLabel(in string text)
                {
                    Text = text;
                    FontSize = 15;
                    TextColor = Color.White;
                    LineHeight = 1.021;
                    LineBreakMode = LineBreakMode.WordWrap;
                    FontFamily = FontFamilyConstants.RobotoRegular;
                    VerticalTextAlignment = TextAlignment.Start;
                    AutomationId = SettingsPageAutomationIds.EnableOrangizationsCarouselDescription;

                    Padding = new Thickness(24, 5);
                }
            }
        }
    }
}
