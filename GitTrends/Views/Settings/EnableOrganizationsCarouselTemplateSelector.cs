using GitTrends.Mobile.Common;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

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

            enum Row { Image, Title, Description }

            static Grid CreateItemsGrid(IncludeOrganizationsCarouselModel includeOrganizationsModel) => new()
            {
                RowDefinitions = Rows.Define(
                    (Row.Image, Stars(4)),
                    (Row.Title, Stars(1)),
                    (Row.Description, Stars(2))),

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
                        .Row(Row.Description)
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

                    Padding = new Thickness(24, 0);
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

                    Padding = new Thickness(24, 0);
                }
            }
        }
    }
}
