using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class EmptyDataView : RelativeLayout
    {
        public const string UnableToRetrieveDataText = "Unable to retrieve data";
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(EmptyDataView), string.Empty);

        public EmptyDataView(in string imageSource, in string automationId)
        {
            AutomationId = automationId;

            var stackLayout = new StackLayout
            {
                Children =
                {
                    new TitleLabel(Text).Bind(Label.TextProperty, nameof(Text), source: this),
                    new EmptyStateImage(imageSource),
                }
            };

            Children.Add(stackLayout,
                        Constraint.RelativeToParent(parent => parent.Width / 2 - GetWidth(parent, stackLayout) / 2),
                        Constraint.RelativeToParent(parent => parent.Height / 2 - GetHeight(parent, stackLayout) / 2));


            static double GetWidth(in RelativeLayout parent, in View view) => view.Measure(parent.Width, parent.Height).Request.Width;
            static double GetHeight(in RelativeLayout parent, in View view) => view.Measure(parent.Width, parent.Height).Request.Height;
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
