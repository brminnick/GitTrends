using System;
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
                    (Row.Title, Stars(1) ),
                    (Row.Description, Stars(2))),

                Children =
                {
                    new OpacityOverlay()
                        .Row(Row.Image),

                    includeOrganizationsModel.SvgFileName is not null
                        ? new SvgImage(includeOrganizationsModel.SvgFileName, () => Color.White, 24, 24)
                            .Row(Row.Image)
                        : new Image { Source = includeOrganizationsModel.ImageSource }
                            .Row(Row.Image),

                    new TitleLabel(includeOrganizationsModel.Title)
                        .Row(Row.Title),

                    new DescriptionLabel(includeOrganizationsModel.Text)
                        .Row(Row.Description)
                }
            };

            class OpacityOverlay : View
            {
                public OpacityOverlay() => BackgroundColor = Color.White.MultiplyAlpha(0.25);
            }

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
