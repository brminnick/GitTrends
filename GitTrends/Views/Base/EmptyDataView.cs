using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;
using static GitTrends.MauiService;

namespace GitTrends;

class EmptyDataView : Grid
{
	public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyDataView), string.Empty);
	public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(EmptyDataView), string.Empty);
	public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(string), typeof(EmptyDataView), string.Empty);

	public EmptyDataView(in string imageSource, in string automationId) : this(automationId)
	{
		ImageSource = imageSource;
	}

	//Workaround for https://github.com/xamarin/Xamarin.Forms/issues/10551
	public EmptyDataView(in string automationId)
	{
		AutomationId = automationId;

		this.Center();

		Margin = 8;
		Padding = 12;
		RowSpacing = 24;

		RowDefinitions = Rows.Define(
			(Row.Text, Stars(1)),
			(Row.Image, Stars(2)));

		Children.Add(new TextLabel
		(
			new Span().Bind(Span.TextProperty, 
				getter: static dataView => dataView.Title, 
				source: this),
			new Span().Bind(Span.TextProperty, 
				getter: static dataView => dataView.Description, 
				source: this)
		).Row(Row.Text));

		Children.Add(new EmptyStateImage(ImageSource)
			.Row(Row.Image).Top()
			.Bind(Image.SourceProperty, 
				getter: static dataView => dataView.ImageSource, 
				source: this));
	}

	enum Row { Text, Image }

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public string Description
	{
		get => (string)GetValue(DescriptionProperty);
		set => SetValue(DescriptionProperty, value);
	}

	public string ImageSource
	{
		get => (string)GetValue(ImageSourceProperty);
		set => SetValue(ImageSourceProperty, value);
	}

	sealed class TextLabel : Label
	{
		public TextLabel(in Span title, in Span description)
		{
			title.FontSize = IsSmallScreen ? 20 : 24;
			title.FontFamily = FontFamilyConstants.RobotoMedium;

			description.FontSize = IsSmallScreen ? 16 : 20;
			description.FontFamily = FontFamilyConstants.RobotoMedium;

			FormattedText = new FormattedString
			{
				Spans =
				{
					title,
					IsSmallScreen
						? new Span { Text = Environment.NewLine }.Font(size: 24)
						: new Span { Text = Environment.NewLine + Environment.NewLine }.Font(size: 24),
					description
				}
			};

			HorizontalOptions = LayoutOptions.Center;
			VerticalOptions = LayoutOptions.End;

			HorizontalTextAlignment = TextAlignment.Center;
			VerticalTextAlignment = TextAlignment.End;

			this.DynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
		}
	}

	sealed class EmptyStateImage : Image
	{
		public EmptyStateImage(in string source)
		{
			Source = source;

			HorizontalOptions = LayoutOptions.Center;
			VerticalOptions = LayoutOptions.Start;

			WidthRequest = HeightRequest = 250;
		}
	}
}