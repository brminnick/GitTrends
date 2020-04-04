using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    abstract class BaseOnboardingPage : ContentPage
    {
        public BaseOnboardingPage(string backgroundColorHex, string nextButtonText)
        {
            BackgroundColor = Color.FromHex(backgroundColorHex);

            var imageView = CreateImageView();
            imageView.Margin = new Thickness(30);

            var descriptionLayout = new StackLayout
            {
                Spacing = 24,
                Margin = new Thickness(30, 10),
                Children =
                {
                    CreateDescriptionTitleLabel(),
                    CreateDescriptionBodyView()
                }
            };

            Content = new Grid
            {
                RowDefinitions = Rows.Define(
                    (Row.ImageRow, StarGridLength(9)),
                    (Row.DescriptionRow, StarGridLength(6)),
                    (Row.IndicatorRow, StarGridLength(1))),

                ColumnDefinitions = Columns.Define(
                    (Column.IndicatorColumn, StarGridLength(1)),
                    (Column.ButtonColumn, StarGridLength(1))),

                Children =
                {
                    new OpacityOverlay().Row(Row.ImageRow).ColumnSpan(All<Column>()),
                    imageView.Row(Row.ImageRow).ColumnSpan(All<Column>()),
                    descriptionLayout.Row(Row.DescriptionRow).ColumnSpan(All<Column>()),
                    new OnboardingIndicatorView().Row(Row.IndicatorRow).Column(Column.IndicatorColumn),
                    new NextButton(nextButtonText).Row(Row.IndicatorRow).Column(Column.ButtonColumn),
                }
            };
        }

        enum Row { ImageRow, DescriptionRow, IndicatorRow }
        enum Column { IndicatorColumn, ButtonColumn }

        protected abstract View CreateImageView();
        protected abstract TitleLabel CreateDescriptionTitleLabel();
        protected abstract View CreateDescriptionBodyView();

        class NextButton : Button
        {
            public NextButton(in string text)
            {
                Margin = new Thickness(0, 0, 30, 0);
                TextColor = Color.White;
                HorizontalOptions = LayoutOptions.End;
                Text = text;
                BackgroundColor = Color.Transparent;
                FontFamily = FontFamilyConstants.RobotoBold;
            }
        }

        protected class BodySvg : SvgImage
        {
            public BodySvg(in string svgFileName) : base(svgFileName, () => Color.White)
            {
            }
        }

        protected class TitleLabel : Label
        {
            public TitleLabel(in string text)
            {
                Text = text;
                FontSize = 34;
                TextColor = Color.White;
                FontFamily = FontFamilyConstants.RobotoBold;
            }
        }

        protected class BodyLabel : Label
        {
            public BodyLabel(in string text)
            {
                Text = text;
                FontSize = 16;
                TextColor = Color.White;
                FontFamily = FontFamilyConstants.RobotoRegular;
            }
        }

        class OpacityOverlay : BoxView
        {
            public OpacityOverlay() => BackgroundColor = Color.FromRgba(255, 255, 255, 0.25);
        }

        class OnboardingIndicatorView : IndicatorView
        {
            public OnboardingIndicatorView()
            {
                Margin = new Thickness(30, 0, 0, 0);
                Count = 4;
                HorizontalOptions = LayoutOptions.Start;

                this.SetBinding(IndicatorView.PositionProperty, nameof(OnboardingViewModel.CurrentPageIndex));
            }
        }
    }
}
