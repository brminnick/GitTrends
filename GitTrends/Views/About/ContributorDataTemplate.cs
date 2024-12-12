using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

/* Unmerged change from project 'GitTrends(net8.0-android)'
Before:
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
				new AvatarImage(_circleDiameter).Fill()
					.Row(Row.Avatar).Column(Column.Image)
					.Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime),

				new Label { LineBreakMode = LineBreakMode.TailTruncation }.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
					.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
					.Bind<Label, string, string>(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime, convert: static  login => $"@{login}")
					.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
			}
		}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
	}
After:
namespace GitTrends;

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
			new AvatarImage(_circleDiameter).Fill()
				.Row(Row.Avatar).Column(Column.Image)
				.Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime),

			new Label { LineBreakMode = LineBreakMode.TailTruncation }.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
				.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
				.Bind<Label, string, string>(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime, convert: static  login => $"@{login}")
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
		}
	}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
*/

/* Unmerged change from project 'GitTrends(net8.0-ios)'
Before:
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
				new AvatarImage(_circleDiameter).Fill()
					.Row(Row.Avatar).Column(Column.Image)
					.Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime),

				new Label { LineBreakMode = LineBreakMode.TailTruncation }.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
					.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
					.Bind<Label, string, string>(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime, convert: static  login => $"@{login}")
					.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
			}
		}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
	}
After:
namespace GitTrends;

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
			new AvatarImage(_circleDiameter).Fill()
				.Row(Row.Avatar).Column(Column.Image)
				.Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime),

			new Label { LineBreakMode = LineBreakMode.TailTruncation }.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
				.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
				.Bind<Label, string, string>(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime, convert: static  login => $"@{login}")
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
		}
	}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
*/

/* Unmerged change from project 'GitTrends(net8.0-maccatalyst)'
Before:
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
				new AvatarImage(_circleDiameter).Fill()
					.Row(Row.Avatar).Column(Column.Image)
					.Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime),

				new Label { LineBreakMode = LineBreakMode.TailTruncation }.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
					.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
					.Bind<Label, string, string>(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime, convert: static  login => $"@{login}")
					.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
			}
		}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
	}
After:
namespace GitTrends;

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
			new AvatarImage(_circleDiameter).Fill()
				.Row(Row.Avatar).Column(Column.Image)
				.Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime),

			new Label { LineBreakMode = LineBreakMode.TailTruncation }.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
				.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
				.Bind<Label, string, string>(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime, convert: static  login => $"@{login}")
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
		}
	}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
*/
using static GitTrends.MauiService;

namespace GitTrends;

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
			new AvatarImage(_circleDiameter).Fill()
				.Row(Row.Avatar).Column(Column.Image)
				.Bind(CircleImage.ImageSourceProperty, 
					getter: static (Contributor vm) => vm.AvatarUrl, 
					mode: BindingMode.OneTime),

			new Label { LineBreakMode = LineBreakMode.TailTruncation }.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
				.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
				.Bind(Label.TextProperty, 
					getter: static (Contributor vm) => vm.Login, 
					mode: BindingMode.OneTime, 
					convert: static  login => $"@{login}")
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
		}
	}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
}