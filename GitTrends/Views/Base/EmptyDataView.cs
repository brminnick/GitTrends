using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class EmptyDataView : StackLayout
    {
        public const string UnableToRetrieveDataText = "Unable to retrieve data";
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(EmptyDataView), string.Empty);

        public EmptyDataView(in string imageSource, in string automationId)
        {
            AutomationId = automationId;

            this.Center();

            BackgroundColor = Color.Green;

            Children.Add(new TitleLabel(Text).Bind(Label.TextProperty, nameof(Text), source: this));
            Children.Add(new EmptyStateImage(imageSource));
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
                BackgroundColor = Color.Pink;

                Source = source;

                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Start;

                WidthRequest = HeightRequest = 250;
            }
        }
    }
}
