using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    abstract class BaseOrganizationsView : Grid
    {
        protected BaseOrganizationsView()
        {
            Padding = new Thickness(20);
            RowDefinitions = Rows.Define(
                (Row.Image, Star),
                (Row.Label, Star));

            Children.Add(CreateImage().Row(Row.Image));
            Children.Add(CreateLabel().Row(Row.Label));
        }

        enum Row { Image, Label }

        protected abstract Image CreateImage();
        protected abstract Label CreateLabel();
    }
}
