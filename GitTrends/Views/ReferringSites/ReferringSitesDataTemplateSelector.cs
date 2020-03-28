using FFImageLoading.Forms;
using GitTrends.Shared;
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
            public ReferringSitesDataTemplate(MobileReferringSiteModel referringSiteModel) : base(() => CreateReferringSitesDataTemplate(referringSiteModel))
            {
            }

            static View CreateReferringSitesDataTemplate(in MobileReferringSiteModel referringSiteModel) => new CardView
            {
                Content = new Grid
                {

                    RowDefinitions = Rows.Define(
                        (Row.Title, Auto),
                        (Row.Description, Auto)),

                    ColumnDefinitions = Columns.Define(
                        (Column.FavIcon, 32),
                        (Column.FavIconPadding, 16),
                        (Column.Site, StarGridLength(1)),
                        (Column.Referrals, Auto),
                        (Column.Separator, AbsoluteGridLength(1)),
                        (Column.Uniques, Auto)),

                    Children =
                    {
                        new FavIconImage(referringSiteModel.FavIcon).Row(Row.Title).Column(Column.FavIcon).RowSpan(2),
                        new TitleLabel("SITE").Row(Row.Title).Column(Column.Site).Start().Margin(new Thickness(0,0,16,0)),
                        new PrimaryColorLabel(referringSiteModel.Referrer).Row(Row.Description).Column(Column.Site).Start().Margin(new Thickness(0,0,16,0)),
                        new TitleLabel("REFERRALS").Row(Row.Title).Column(Column.Referrals).Center(),
                        new PrimaryColorLabel(referringSiteModel.TotalCount.ToString()).Row(Row.Description).Column(Column.Referrals).Center()
                        .Bind(Label.TextProperty, nameof(ReferringSiteModel.TotalCount), converter: NumberConverter),
                        new Separator().Row(Row.Title).Column(Column.Separator).RowSpan(2).FillExpandVertical(),
                        new TitleLabel("UNIQUE").Row(Row.Title).Column(Column.Uniques).Center(),
                        new PrimaryColorLabel(referringSiteModel.TotalUniqueCount.ToString()).Row(Row.Description).Column(Column.Uniques).Center()
                        .Bind(Label.TextProperty, nameof(ReferringSiteModel.TotalUniqueCount), converter: NumberConverter),
                    }
                }
            };

            class CardView : Frame
            {
                public CardView()
                {
                    CornerRadius = 4;
                    HasShadow = false;
                    Visual = VisualMarker.Material;
                    Padding = new Thickness(16);

                    SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                    SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
                }
            }

            class FavIconImage : CachedImage
            {
                public FavIconImage(ImageSource imageSource)
                {
                    HeightRequest = MobileReferringSiteModel.FavIconSize;
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    VerticalOptions = LayoutOptions.FillAndExpand;
                    LoadingPlaceholder = FavIconService.DefaultFavIcon;
                    ErrorPlaceholder = FavIconService.DefaultFavIcon;
                    Source = imageSource;
                }
            }

            class TitleLabel : PrimaryColorLabel
            {

                public TitleLabel(in string text) : base(text)
                {
                    FontSize = 12;
                    CharacterSpacing = 1.56;
                    HorizontalTextAlignment = TextAlignment.Start;
                    VerticalTextAlignment = TextAlignment.Start;
                    HorizontalOptions = LayoutOptions.FillAndExpand;

                    SetDynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
                    SetDynamicResource(FontFamilyProperty, nameof(BaseTheme.RobotoMedium));
                }
            }

            class PrimaryColorLabel : Label
            {
                public PrimaryColorLabel(in double fontSize, in string text) : this(text) => FontSize = fontSize;

                public PrimaryColorLabel(in string text)
                {
                    Text = text;
                    FontSize = 12;
                    MaxLines = 1;
                    LineBreakMode = LineBreakMode.TailTruncation;
                    HorizontalTextAlignment = TextAlignment.Start;

                    SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
                    SetDynamicResource(FontFamilyProperty, nameof(BaseTheme.RobotoRegular));
                }
            }

            class Separator : BoxView
            {
                public Separator()
                {
                    SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
                }
            }

            static FuncConverter<object, string> NumberConverter => new FuncConverter<object, string>(value =>
            {
                if (value == null) return "0";

                double number = 0;
                if (double.TryParse(value.ToString(), out number))
                {
                    if (number < 10e2)
                        return string.Format("{0:0}", number);
                    else if (number < 10e5)
                        return $"{string.Format("{0:0.0}", number / 10e2)}K";
                    else if (number < 10e8)
                        return $"{string.Format("{0:0.0}", number / 10e5)}M";
                    else if (number < 10e11)
                        return $"{string.Format("{0:0.0}", number / 10e8)}B";
                }

                return "0";
            });

            enum Row { Title, Description }
            enum Column { FavIcon, FavIconPadding, Site, Referrals, Separator, Uniques }
        }
    }
}