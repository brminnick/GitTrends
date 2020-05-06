using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class EmptyDataView : AbsoluteLayout
    {
        public const string UnableToRetrieveDataText = "Unable to retrieve data";
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(EmptyDataView), string.Empty);

        public EmptyDataView(in string imageSource, in string automationId)
        {
            AutomationId = automationId;

            var stackLayout = new StackLayout
            {
                Spacing = 12,
                Children =
                {
                    new TitleLabel(Text).Bind(Label.TextProperty, nameof(Text), source: this),
                    new EmptyStateImage(imageSource),
                }
            };

            //Workaround for https://github.com/xamarin/Xamarin.Forms/issues/10551
            Children.Add(stackLayout, new Rectangle(.5, .5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        class TitleLabel : Label
        {
            public TitleLabel(in string text)
            {
                Text = text;

                FontSize = 24;
                FontFamily = FontFamilyConstants.RobotoMedium;

                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.End;

                HorizontalTextAlignment = TextAlignment.Center;
                VerticalTextAlignment = TextAlignment.End;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
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
