using FFImageLoading.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class ReferringSitesDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => new ReferringSitesDataTemplate((MobileReferringSiteModel)item);

        class ReferringSitesDataTemplate : DataTemplate
        {
            const int rowHeight = Shared.ReferringSiteModel.FavIconSize + 10;

            public ReferringSitesDataTemplate(MobileReferringSiteModel referringSiteModel) : base(() => CreateReferringSitesDataTemplate(referringSiteModel))
            {
            }

            enum Row { TopPadding, Title, Description, BotomPadding }
            enum Column { LeftPadding, FavIcon, Site, Referrals, Uniques, RightPadding }

            static View CreateReferringSitesDataTemplate(in MobileReferringSiteModel referringSiteModel)
            {
                var referringSitesGrid = new Grid
                {
                    RowSpacing = 1,
                    Margin = new Thickness(5),

                    RowDefinitions = Rows.Define(
                        (Row.TopPadding, AbsoluteGridLength(2)),
                        (Row.Title, AbsoluteGridLength(rowHeight / 2)),
                        (Row.Description, AbsoluteGridLength(rowHeight / 2)),
                        (Row.BotomPadding, AbsoluteGridLength(2))),

                    ColumnDefinitions = Columns.Define(
                        (Column.LeftPadding, AbsoluteGridLength(5)),
                        (Column.FavIcon, AbsoluteGridLength(rowHeight)),
                        (Column.Site, StarGridLength(3)),
                        (Column.Referrals, StarGridLength(2)),
                        (Column.Uniques, StarGridLength(2)),
                        (Column.RightPadding, AbsoluteGridLength(5))),

                    Children =
                    {
                        new FavIconImage(referringSiteModel.FavIcon).Row(Row.Title).Column(Column.FavIcon).RowSpan(4),
                        new TitleLabel("Site").Row(Row.Title).Column(Column.Site),
                        new DescriptionLabel(referringSiteModel.Referrer).Row(Row.Description).Column(Column.Site),
                        new TitleLabel("Referrals") { LineBreakMode = LineBreakMode.TailTruncation }.Row(Row.Title).Column(Column.Referrals),
                        new DescriptionLabel(referringSiteModel.TotalCount.ToString()).Row(Row.Description).Column(Column.Referrals),
                        new TitleLabel("Unique Visitors").Row(Row.Title).Column(Column.Uniques),
                        new DescriptionLabel(referringSiteModel.TotalUniqueCount.ToString()).Row(Row.Description).Column(Column.Uniques),
                    }
                };

                referringSitesGrid.SetDynamicResource(VisualElement.BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
                return referringSitesGrid;
            }

            class FavIconImage : CachedImage
            {
                public FavIconImage(ImageSource imageSource)
                {
                    HeightRequest = MobileReferringSiteModel.FavIconSize;
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    VerticalOptions = LayoutOptions.CenterAndExpand;
                    LoadingPlaceholder = FavIconService.DefaultFavIcon;
                    ErrorPlaceholder = FavIconService.DefaultFavIcon;
                    Source = imageSource;
                }
            }

            class TitleLabel : CenteredLabel
            {
                public TitleLabel(string text)
                {
                    FontAttributes = FontAttributes.Bold;
                    FontSize = 14;
                    Text = text;
                    Padding = new Thickness(0, 2, 0, 0);
                }
            }

            class DescriptionLabel : CenteredLabel
            {
                public DescriptionLabel(string text)
                {
                    Text = text;
                    FontSize = 14;
                    Padding = new Thickness(0, 0, 0, 2);
                }
            }

            abstract class CenteredLabel : Label
            {
                protected CenteredLabel()
                {
                    VerticalOptions = LayoutOptions.Center;
                    HorizontalTextAlignment = TextAlignment.Center;
                    VerticalTextAlignment = TextAlignment.Center;
                    SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));
                }
            }
        }
    }
}