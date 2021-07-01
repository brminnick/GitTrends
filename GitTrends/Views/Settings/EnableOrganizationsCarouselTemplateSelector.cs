using System.Threading;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    class EnableOrganizationsCarouselTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => new EnableOrganizationsCarouselTemplate((IncludeOrganizationsCarouselModel)item, (SettingsViewModel)container.BindingContext);

        class EnableOrganizationsCarouselTemplate : DataTemplate
        {
            public EnableOrganizationsCarouselTemplate(IncludeOrganizationsCarouselModel includeOrganizationsModel, SettingsViewModel settingsViewModel) : base(() => CreateItemsGrid(includeOrganizationsModel, settingsViewModel))
            {

            }

            enum Row { Image, Title, Description, GitHubButton, IndicatorView }

            static Grid CreateItemsGrid(IncludeOrganizationsCarouselModel includeOrganizationsModel, SettingsViewModel settingsViewModel) => new()
            {
                RowSpacing = 0, // Must be zero to match OrganizationsCarouselFrame's Grid

                RowDefinitions = Rows.Define(
                    (Row.Image, Stars(8)), // Must be 1/2 of total height to match OrganizationsCarouselFrame's Grid
                    (Row.Title, Stars(1)),
                    (Row.Description, Stars(3)),
                    (Row.GitHubButton, Stars(2)),
                    (Row.IndicatorView, Stars(2))), // Must be 1/8 of total height to match OrganizationsCarouselFrame's Grid

                Children =
                {
                    includeOrganizationsModel.Url is not null
                        ? new VideoPlayerView()
                            .Row(Row.Image)
                        : new Image { Source = includeOrganizationsModel.ImageSource, Aspect = Aspect.AspectFit }.Margin(24, 12)
                            .Row(Row.Image),

                    new TitleLabel(includeOrganizationsModel.Title)
                        .Row(Row.Title),

                    new DescriptionLabel(includeOrganizationsModel.Text)
                        .Row(Row.Description),

                    new GitHubButton(SettingsPageAutomationIds.GitHubButton, SettingsPageConstants.ManageOrganizations)
                        .Row(Row.GitHubButton)
                        .Bind(GitHubButton.CommandProperty, nameof(SettingsViewModel.ManageOrganizationsButtonCommand), source: settingsViewModel)
                        .Invoke(button =>   button.CommandParameter = (CancellationToken.None, (Xamarin.Essentials.BrowserLaunchOptions?)null)),

                    new BoxView()
                        .Row(Row.IndicatorView)
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
