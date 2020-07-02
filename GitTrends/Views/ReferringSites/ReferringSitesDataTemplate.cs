using System;
using FFImageLoading.Forms;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Sharpnado.MaterialFrame;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class ReferringSitesDataTemplate : DataTemplate
    {
        public ReferringSitesDataTemplate() : base(() => new CardView())
        {
        }

        public static int TopPadding { get; } = 8;
        public static int BottomPadding { get; } = Device.RuntimePlatform is Device.Android ? 12 : 16;

        class CardView : Grid
        {
            public CardView()
            {
                RowSpacing = 0;
                RowDefinitions = Rows.Define(
                    (Row.TopPadding, AbsoluteGridLength(TopPadding)),
                    (Row.Card, StarGridLength(1)),
                    (Row.BottomPadding, AbsoluteGridLength(BottomPadding)));

                ColumnDefinitions = Columns.Define(
                    (Column.LeftPadding, AbsoluteGridLength(16)),
                    (Column.Card, StarGridLength(1)),
                    (Column.RightPadding, AbsoluteGridLength(16)));

                Children.Add(new CardViewFrame().Row(Row.Card).Column(Column.Card));

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            }

            enum Row { TopPadding, Card, BottomPadding }
            enum Column { LeftPadding, Card, RightPadding }

            class CardViewFrame : MaterialFrame
            {
                public CardViewFrame()
                {
                    CornerRadius = 4;
                    HasShadow = false;
                    Padding = new Thickness(16);
                    Elevation = 4;
                    Content = new ContentGrid();

                    SetDynamicResource(MaterialThemeProperty, nameof(BaseTheme.DefaultMaterialFrameTheme));
                }
            }

            class ContentGrid : Grid
            {
                const int _favIconWidth = MobileReferringSiteModel.FavIconSize;
                const int _favIconHeight = MobileReferringSiteModel.FavIconSize;

                public ContentGrid()
                {
                    const int rowSpacing = 6;
                    const int separatorPadding = 12;

                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    VerticalOptions = LayoutOptions.FillAndExpand;

                    RowSpacing = rowSpacing;
                    ColumnSpacing = 0;

                    RowDefinitions = Rows.Define(
                        (Row.Title, AbsoluteGridLength(_favIconHeight / 2 - rowSpacing / 2)),
                        (Row.Description, AbsoluteGridLength(_favIconHeight / 2 - rowSpacing / 2)));

                    ColumnDefinitions = Columns.Define(
                        (Column.FavIcon, AbsoluteGridLength(_favIconWidth)),
                        (Column.FavIconPadding, AbsoluteGridLength(16)),
                        (Column.Site, StarGridLength(1)),
                        (Column.SitePadding, AbsoluteGridLength(8)),
                        (Column.Referrals, AbsoluteGridLength(80)),
                        (Column.ReferralPadding, AbsoluteGridLength(separatorPadding)),
                        (Column.Separator, AbsoluteGridLength(1)),
                        (Column.UniquePadding, AbsoluteGridLength(separatorPadding)),
                        (Column.Uniques, AbsoluteGridLength(55)));

                    Children.Add(new FavIconImage()
                                        .Row(Row.Title).Column(Column.FavIcon).RowSpan(2));

                    Children.Add(new TitleLabel(ReferringSitesPageConstants.Site, TextAlignment.Start, LayoutOptions.Start)
                                        .Row(Row.Title).Column(Column.Site));

                    Children.Add(new DescriptionLabel()
                                        .Row(Row.Description).Column(Column.Site)
                                        .Bind(Label.TextProperty, nameof(MobileReferringSiteModel.Referrer)));

                    Children.Add(new TitleLabel(ReferringSitesPageConstants.Referrals, TextAlignment.End, LayoutOptions.End).Assign(out TitleLabel referralsTitleLabel)
                                        .Row(Row.Title).Column(Column.Referrals));

                    Children.Add(new StatisticsLabel(referralsTitleLabel)
                                        .Row(Row.Description).Column(Column.Referrals)
                                        .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(MobileReferringSiteModel.TotalCount), convert: statisticsConverter));

                    Children.Add(new Separator()
                                        .Row(Row.Title).Column(Column.Separator).RowSpan(2));

                    Children.Add(new TitleLabel(ReferringSitesPageConstants.Unique, TextAlignment.Start, LayoutOptions.Start).Assign(out TitleLabel uniqueTitleLabel)
                                        .Row(Row.Title).Column(Column.Uniques));

                    Children.Add(new StatisticsLabel(uniqueTitleLabel)
                                        .Row(Row.Description).Column(Column.Uniques)
                                        .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(MobileReferringSiteModel.TotalUniqueCount), convert: statisticsConverter));

                    static string statisticsConverter(long number) => number.ConvertToAbbreviatedText();
                }

                enum Row { Title, Description }
                enum Column { FavIcon, FavIconPadding, Site, SitePadding, Referrals, ReferralPadding, Separator, UniquePadding, Uniques }

                class TitleLabel : Label
                {

                    public TitleLabel(in string text, TextAlignment horizontalTextAlignment, LayoutOptions horizontalOptions)
                    {
                        Text = text;
                        MaxLines = 1;
                        FontSize = 12;
                        CharacterSpacing = 1.56;
                        FontFamily = FontFamilyConstants.RobotoMedium;
                        LineBreakMode = LineBreakMode.TailTruncation;

                        HorizontalOptions = horizontalOptions;
                        HorizontalTextAlignment = horizontalTextAlignment;

                        VerticalOptions = LayoutOptions.Start;
                        VerticalTextAlignment = TextAlignment.Start;

                        SetDynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
                    }
                }

                class StatisticsLabel : Label
                {
                    public StatisticsLabel(in TitleLabel titleLabel)
                    {
                        MaxLines = 1;
                        FontSize = 12;
                        FontFamily = FontFamilyConstants.RobotoRegular;
                        LineBreakMode = LineBreakMode.TailTruncation;

                        HorizontalOptions = titleLabel.HorizontalOptions;
                        HorizontalTextAlignment = TextAlignment.Center;

                        VerticalTextAlignment = TextAlignment.End;

                        SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
                        SetBinding(WidthRequestProperty, new Binding(nameof(Width), source: titleLabel));
                    }
                }

                class DescriptionLabel : Label
                {
                    public DescriptionLabel()
                    {
                        MaxLines = 1;
                        FontSize = 12;
                        FontFamily = FontFamilyConstants.RobotoRegular;
                        LineBreakMode = LineBreakMode.TailTruncation;

                        HorizontalOptions = LayoutOptions.FillAndExpand;
                        HorizontalTextAlignment = TextAlignment.Start;

                        VerticalTextAlignment = TextAlignment.End;

                        SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
                    }
                }

                class Separator : BoxView
                {
                    public Separator()
                    {
                        VerticalOptions = LayoutOptions.FillAndExpand;
                        SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
                    }
                }

                class FavIconImage : PancakeView
                {
                    public FavIconImage()
                    {
                        const int padding = 1;

                        this.Start();

                        BackgroundColor = Color.White;

                        Padding = new Thickness(padding);

                        var circleDiameter = Math.Min(_favIconHeight, _favIconWidth);

                        CornerRadius = circleDiameter / 2;
                        HeightRequest = WidthRequest = circleDiameter;

                        Content = new CircleImage
                        {
                            Aspect = Aspect.AspectFill,
                            ErrorPlaceholder = FavIconService.DefaultFavIcon,
                            LoadingPlaceholder = FavIconService.DefaultFavIcon
                        }.Bind(CircleImage.ImageSourceProperty, nameof(MobileReferringSiteModel.FavIcon));
                    }
                }
            }
        }
    }
}