using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    class EnableOrganizationsCarouselTemplate : DataTemplate
    {
        public EnableOrganizationsCarouselTemplate() : base(CreateItemsGrid)
        {
        }

        enum Row { Image, Label, Next }

        static Grid CreateItemsGrid() => new()
        {
            BackgroundColor = Color.Orange,

            RowDefinitions = Rows.Define(
                (Row.Image, Star),
                (Row.Label, Star),
                (Row.Next, 50)),

            Children =
            {
                new Label { Text = "Image"}.Center()
                    .Row(Row.Image),
                new Label { Text = "Text"}.Center()
                    .Row(Row.Label),
                new Label { Text = "Next"}.Center()
                    .Row(Row.Next)
            }
        };
    }
}
