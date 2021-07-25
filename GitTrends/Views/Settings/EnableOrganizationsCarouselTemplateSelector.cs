using System.Runtime.CompilerServices;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace GitTrends
{
    class EnableOrganizationsCarouselTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => new EnableOrganizationsCarouselTemplate((IncludeOrganizationsCarouselModel)item, (CarouselView)container);

        class EnableOrganizationsCarouselTemplate : DataTemplate
        {
            public EnableOrganizationsCarouselTemplate(IncludeOrganizationsCarouselModel includeOrganizationsModel, CarouselView organizationsCarouselView) : base(() => CreateItemsGrid(includeOrganizationsModel, organizationsCarouselView))
            {

            }

            static EnableOrganizationsGrid CreateItemsGrid(IncludeOrganizationsCarouselModel includeOrganizationsModel, CarouselView organizationsCarouselView) => new()
            {
                Children =
                {
                    includeOrganizationsModel.Url is not null
                        ? new VideoPlayerView(includeOrganizationsModel.Url).Margin(24, 12)
                            .Row(EnableOrganizationsGrid.Row.Image)
                        : new Image { Source = includeOrganizationsModel.ImageSource, Aspect = Aspect.AspectFit }.Margin(24, 12)
                            .Row(EnableOrganizationsGrid.Row.Image),

                    new TitleLabel(includeOrganizationsModel.Title)
                        .Row(EnableOrganizationsGrid.Row.Title),

                    new DescriptionLabel(includeOrganizationsModel.Text)
                        .Row(EnableOrganizationsGrid.Row.Description),

                    new GitHubButton(SettingsPageAutomationIds.GitHubButton, SettingsPageConstants.ManageOrganizations) { IsVisible = false }
                        .Row(EnableOrganizationsGrid.Row.GitHubButton)
                        .Bind(GitHubButton.CommandProperty, nameof(SettingsViewModel.ManageOrganizationsButtonCommand), source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(SettingsViewModel)))
                        .Invoke(button =>
                        {
                            organizationsCarouselView.Scrolled += async (s, e) =>
                            {
                                var shouldDisplayButton = e.CenterItemIndex is 2
                                                            && organizationsCarouselView.Position is 2
                                                            && organizationsCarouselView.IsDragging is false;
                                if(shouldDisplayButton)
                                {
                                    button.Opacity = 0;
                                    button.IsVisible = true;
                                    await button.FadeTo(1, 1000);
                                }
                                else
                                {
                                    button.IsVisible = false;
                                }
                            };
                        }),

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
