using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

/* Unmerged change from project 'GitTrends(net8.0-android)'
Before:
namespace GitTrends
{
	class LibraryDataTemplate() : DataTemplate(CreateLibraryDataTemplate)
	{
		const int _rowSpacing = 4;
		const int _loginTextHeight = 24;
		const int _textPadding = 8;

		static readonly int _circleDiameter = IsSmallScreen ? 54 : 64;

		public static int RowHeight { get; } = _rowSpacing + _circleDiameter + _loginTextHeight;

		enum Row { Avatar, Login }
		enum Column { LeftText, Image, RightText, RightPadding }

		static Grid CreateLibraryDataTemplate() => new Grid
		{
			RowSpacing = 4,

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
				new AvatarImage(_circleDiameter)
					{
						BackgroundColor = Colors.White,
						Padding = 12
					}.Fill()
					.Row(Row.Avatar).Column(Column.Image)
					.Bind(CircleImage.ImageSourceProperty, nameof(NuGetPackageModel.IconUri), BindingMode.OneTime),

				new Label
					{
						LineBreakMode = LineBreakMode.TailTruncation
					}.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
					.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
					.Bind(Label.TextProperty, nameof(NuGetPackageModel.PackageName), BindingMode.OneTime)
					.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
			}
		}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
	}
After:
namespace GitTrends;

class LibraryDataTemplate() : DataTemplate(CreateLibraryDataTemplate)
{
	const int _rowSpacing = 4;
	const int _loginTextHeight = 24;
	const int _textPadding = 8;

	static readonly int _circleDiameter = IsSmallScreen ? 54 : 64;

	public static int RowHeight { get; } = _rowSpacing + _circleDiameter + _loginTextHeight;

	enum Row { Avatar, Login }
	enum Column { LeftText, Image, RightText, RightPadding }

	static Grid CreateLibraryDataTemplate() => new Grid
	{
		RowSpacing = 4,

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
			new AvatarImage(_circleDiameter)
				{
					BackgroundColor = Colors.White,
					Padding = 12
				}.Fill()
				.Row(Row.Avatar).Column(Column.Image)
				.Bind(CircleImage.ImageSourceProperty, nameof(NuGetPackageModel.IconUri), BindingMode.OneTime),

			new Label
				{
					LineBreakMode = LineBreakMode.TailTruncation
				}.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
				.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
				.Bind(Label.TextProperty, nameof(NuGetPackageModel.PackageName), BindingMode.OneTime)
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
		}
	}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
*/

/* Unmerged change from project 'GitTrends(net8.0-ios)'
Before:
namespace GitTrends
{
	class LibraryDataTemplate() : DataTemplate(CreateLibraryDataTemplate)
	{
		const int _rowSpacing = 4;
		const int _loginTextHeight = 24;
		const int _textPadding = 8;

		static readonly int _circleDiameter = IsSmallScreen ? 54 : 64;

		public static int RowHeight { get; } = _rowSpacing + _circleDiameter + _loginTextHeight;

		enum Row { Avatar, Login }
		enum Column { LeftText, Image, RightText, RightPadding }

		static Grid CreateLibraryDataTemplate() => new Grid
		{
			RowSpacing = 4,

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
				new AvatarImage(_circleDiameter)
					{
						BackgroundColor = Colors.White,
						Padding = 12
					}.Fill()
					.Row(Row.Avatar).Column(Column.Image)
					.Bind(CircleImage.ImageSourceProperty, nameof(NuGetPackageModel.IconUri), BindingMode.OneTime),

				new Label
					{
						LineBreakMode = LineBreakMode.TailTruncation
					}.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
					.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
					.Bind(Label.TextProperty, nameof(NuGetPackageModel.PackageName), BindingMode.OneTime)
					.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
			}
		}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
	}
After:
namespace GitTrends;

class LibraryDataTemplate() : DataTemplate(CreateLibraryDataTemplate)
{
	const int _rowSpacing = 4;
	const int _loginTextHeight = 24;
	const int _textPadding = 8;

	static readonly int _circleDiameter = IsSmallScreen ? 54 : 64;

	public static int RowHeight { get; } = _rowSpacing + _circleDiameter + _loginTextHeight;

	enum Row { Avatar, Login }
	enum Column { LeftText, Image, RightText, RightPadding }

	static Grid CreateLibraryDataTemplate() => new Grid
	{
		RowSpacing = 4,

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
			new AvatarImage(_circleDiameter)
				{
					BackgroundColor = Colors.White,
					Padding = 12
				}.Fill()
				.Row(Row.Avatar).Column(Column.Image)
				.Bind(CircleImage.ImageSourceProperty, nameof(NuGetPackageModel.IconUri), BindingMode.OneTime),

			new Label
				{
					LineBreakMode = LineBreakMode.TailTruncation
				}.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
				.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
				.Bind(Label.TextProperty, nameof(NuGetPackageModel.PackageName), BindingMode.OneTime)
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
		}
	}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
*/

/* Unmerged change from project 'GitTrends(net8.0-maccatalyst)'
Before:
namespace GitTrends
{
	class LibraryDataTemplate() : DataTemplate(CreateLibraryDataTemplate)
	{
		const int _rowSpacing = 4;
		const int _loginTextHeight = 24;
		const int _textPadding = 8;

		static readonly int _circleDiameter = IsSmallScreen ? 54 : 64;

		public static int RowHeight { get; } = _rowSpacing + _circleDiameter + _loginTextHeight;

		enum Row { Avatar, Login }
		enum Column { LeftText, Image, RightText, RightPadding }

		static Grid CreateLibraryDataTemplate() => new Grid
		{
			RowSpacing = 4,

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
				new AvatarImage(_circleDiameter)
					{
						BackgroundColor = Colors.White,
						Padding = 12
					}.Fill()
					.Row(Row.Avatar).Column(Column.Image)
					.Bind(CircleImage.ImageSourceProperty, nameof(NuGetPackageModel.IconUri), BindingMode.OneTime),

				new Label
					{
						LineBreakMode = LineBreakMode.TailTruncation
					}.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
					.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
					.Bind(Label.TextProperty, nameof(NuGetPackageModel.PackageName), BindingMode.OneTime)
					.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
			}
		}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
	}
After:
namespace GitTrends;

class LibraryDataTemplate() : DataTemplate(CreateLibraryDataTemplate)
{
	const int _rowSpacing = 4;
	const int _loginTextHeight = 24;
	const int _textPadding = 8;

	static readonly int _circleDiameter = IsSmallScreen ? 54 : 64;

	public static int RowHeight { get; } = _rowSpacing + _circleDiameter + _loginTextHeight;

	enum Row { Avatar, Login }
	enum Column { LeftText, Image, RightText, RightPadding }

	static Grid CreateLibraryDataTemplate() => new Grid
	{
		RowSpacing = 4,

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
			new AvatarImage(_circleDiameter)
				{
					BackgroundColor = Colors.White,
					Padding = 12
				}.Fill()
				.Row(Row.Avatar).Column(Column.Image)
				.Bind(CircleImage.ImageSourceProperty, nameof(NuGetPackageModel.IconUri), BindingMode.OneTime),

			new Label
				{
					LineBreakMode = LineBreakMode.TailTruncation
				}.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
				.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
				.Bind(Label.TextProperty, nameof(NuGetPackageModel.PackageName), BindingMode.OneTime)
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
		}
	}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
*/
using static GitTrends.MauiService;

namespace GitTrends;

class LibraryDataTemplate() : DataTemplate(CreateLibraryDataTemplate)
{
	const int _rowSpacing = 4;
	const int _loginTextHeight = 24;
	const int _textPadding = 8;

	static readonly int _circleDiameter = IsSmallScreen ? 54 : 64;

	public static int RowHeight { get; } = _rowSpacing + _circleDiameter + _loginTextHeight;

	enum Row { Avatar, Login }
	enum Column { LeftText, Image, RightText, RightPadding }

	static Grid CreateLibraryDataTemplate() => new Grid
	{
		RowSpacing = 4,

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
			new AvatarImage(_circleDiameter)
				{
					BackgroundColor = Colors.White,
					Padding = 12
				}.Fill()
				.Row(Row.Avatar).Column(Column.Image)
				.Bind(CircleImage.ImageSourceProperty, 
					getter: static (NuGetPackageModel vm) => vm.IconUri, 
					mode: BindingMode.OneTime),

			new Label
				{
					LineBreakMode = LineBreakMode.TailTruncation
				}.FillHorizontal().TextTop().TextCenterHorizontal().Font(FontFamilyConstants.RobotoRegular, IsSmallScreen ? 10 : 12)
				.Row(Row.Login).Column(Column.LeftText).ColumnSpan(3)
				.Bind(Label.TextProperty, 
					getter: static (NuGetPackageModel vm) => vm.PackageName, 
					mode: BindingMode.OneTime)
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
		}
	}.DynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
}