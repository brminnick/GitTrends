using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    class EnableOrganizationsGrid : Grid
    {
        public EnableOrganizationsGrid()
        {
            RowSpacing = 0;

            RowDefinitions = Rows.Define(
                (Row.Image, Stars(8)),
                (Row.Title, Stars(1)),
                (Row.Description, Stars(3)),
                (Row.GitHubButton, Stars(2)),
                (Row.IndicatorView, Stars(2)));
        }

        public enum Row { Image, Title, Description, GitHubButton, IndicatorView }
    }
}
