using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static GitTrends.XamarinFormsService;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
	class ContributorDataTemplate : DataTemplate
	{
		const int _rowSpacing = 4;
		const int _loginTextHeight = 25;
		const int _textPadding = 4;

		static readonly int _circleDiameter = IsSmallScreen ? 54 : 64;

		public ContributorDataTemplate() : base(CreateContributorDataTemplate)
		{

		}

		public static int RowHeight { get; } = _rowSpacing + _circleDiameter + _loginTextHeight;

		enum Row { Avatar, Login }
		enum Column { LeftText, Image, RightText, RightPadding }

		static Grid CreateContributorDataTemplate() => new Grid
		{
			RowSpacing = _rowSpacing,

			RowDefinitions = Rows.Define(
				(Row.Avatar, _circleDiameter),
				(Row.Login, _loginTextHeight)),

			ColumnDefinitions = Columns.Define(
				(Column.LeftText, _textPadding),
				(Column.Image, _circleDiameter),
				(Column.RightText, _textPadding),
				(Column.RightPadding, 0.5)),

			Children =
			{
				new AvatarImage(_circleDiameter).FillExpand()
					.Row(Row.Avatar).Column(Column.Image)
					.Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime)
					.DynamicResource(CircleImage.BorderColorProperty, nameof(BaseTheme.SeparatorColor)),

				new Label { LineBreakMode = LineBreakMode.TailTruncation }.FillExpandHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
					.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
					.Bind<Label, string, string>(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime, convert: static  login => $"@{login}")
					.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
			}
		}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
	}
}