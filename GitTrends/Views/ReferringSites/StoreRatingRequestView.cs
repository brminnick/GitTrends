using GitTrends.Mobile.Common;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class StoreRatingRequestView : Grid
    {
        public StoreRatingRequestView()
        {
            CascadeInputTransparent = false;
            InputTransparent = true;

            Padding = new Thickness(48, 6, 48, Device.RuntimePlatform is Device.iOS ? 36 : 12);

            RowSpacing = 8;
            ColumnSpacing = 12;

            RowDefinitions = Rows.Define(
                (Row.Title, AbsoluteGridLength(50)),
                (Row.Buttons, AbsoluteGridLength(40)));
            ColumnDefinitions = Columns.Define(
                (Column.No, Star),
                (Column.Yes, Star));

            Children.Add(new RequestTitleLabel().Row(Row.Title).ColumnSpan(All<Column>())
                                .Bind(Label.TextProperty, nameof(ReferringSitesViewModel.ReviewRequestView_TitleLabel)));

            Children.Add(new NoButton().Row(Row.Buttons).Column(Column.No)
                                .Bind(Button.TextProperty, nameof(ReferringSitesViewModel.ReviewRequestView_NoButtonText))
                                .Bind(Button.CommandProperty, nameof(ReferringSitesViewModel.NoButtonCommand)));

            Children.Add(new YesButton().Row(Row.Buttons).Column(Column.Yes)
                                .Bind(Button.TextProperty, nameof(ReferringSitesViewModel.ReviewRequestView_YesButtonText))
                                .Bind(Button.CommandProperty, nameof(ReferringSitesViewModel.YesButtonCommand)));

            this.SetBinding(IsVisibleProperty, nameof(ReferringSitesViewModel.IsStoreRatingRequestVisible));
            this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor_85Opactity));
        }

        enum Row { Title, Buttons }
        enum Column { No, Yes }

        class NoButton : BorderButton
        {
            public NoButton() : base(ReferringSitesPageAutomationIds.StoreRatingRequestNoButton)
            {
                InputTransparent = false;
                FontFamily = FontFamilyConstants.RobotoRegular;

                RemoveDynamicResource(TextColorProperty);
                this.DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));

                RemoveDynamicResource(BackgroundColorProperty);
                this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor_85Opactity));
            }
        }

        class YesButton : BorderButton
        {
            public YesButton() : base(ReferringSitesPageAutomationIds.StoreRatingRequestYesButton)
            {
                InputTransparent = false;
                FontFamily = FontFamilyConstants.RobotoBold;
            }
        }

        class RequestTitleLabel : TitleLabel
        {
            public RequestTitleLabel()
            {
                FontSize = 18;
                InputTransparent = true;
                LineBreakMode = LineBreakMode.WordWrap;
                HorizontalTextAlignment = TextAlignment.Center;
                AutomationId = ReferringSitesPageAutomationIds.StoreRatingRequestTitleLabel;
            }
        }
    }
}
