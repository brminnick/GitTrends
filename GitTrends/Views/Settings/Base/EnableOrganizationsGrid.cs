using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

class EnableOrganizationsGrid : Grid
{
	public EnableOrganizationsGrid()
	{
		Padding = 0;
		RowSpacing = 0;

		RowDefinitions = Rows.Define(
			(Row.Image, Stars(10)),
			(Row.Title, Stars(2)),
			(Row.Description, Stars(3)),
			(Row.GitHubButton, Stars(2)),
			(Row.IndicatorView, Stars(2)));
	}

	public enum Row { Image, Title, Description, GitHubButton, IndicatorView }
}