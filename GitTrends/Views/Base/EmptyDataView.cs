using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class EmptyDataView : AbsoluteLayout
    {
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyDataView), string.Empty);
        public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(EmptyDataView), string.Empty);
        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(string), typeof(EmptyDataView), string.Empty);

        public EmptyDataView(in string imageSource, in string automationId) : this(automationId)
        {
            ImageSource = imageSource;
        }

        public EmptyDataView(in string automationId)
        {
            AutomationId = automationId;

            var stackLayout = new StackLayout
            {
                Spacing = 24,
                Children =
                {
                    new TextLabel
                    (
                        new Span().Bind(Span.TextProperty, nameof(Title), source: this),
                        new Span().Bind(Span.TextProperty, nameof(Description), source: this)
                    ),
                    new EmptyStateImage(ImageSource).Bind(Image.SourceProperty, nameof(ImageSource), source: this)
                }
            };

            //Workaround for https://github.com/xamarin/Xamarin.Forms/issues/10551
            Children.Add(stackLayout, new Rectangle(.5, .5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
        }

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

        class TextLabel : Label
        {
            public TextLabel(Span title, Span description)
            {
                title.FontSize = 24;
                title.FontFamily = FontFamilyConstants.RobotoMedium;

                description.FontSize = 20;
                description.FontFamily = FontFamilyConstants.RobotoMedium;

                FormattedText = new FormattedString
                {
                    Spans =
                    {
                        title,
                        new Span { Text = "\n" },
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

        class EmptyStateImage : Image
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
}
