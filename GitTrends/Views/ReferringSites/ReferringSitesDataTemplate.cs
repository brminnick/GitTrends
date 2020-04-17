using System;
using GitTrends.Mobile.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Essentials;
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

        class CardView : Grid
        {
            public CardView()
            {
                RowSpacing = 0;
                RowDefinitions = Rows.Define(
                    (Row.TopPadding, AbsoluteGridLength(8)),
                    (Row.Card, StarGridLength(1)),
                    (Row.BottomPadding, AbsoluteGridLength(8)));

                ColumnDefinitions = Columns.Define(
                    (Column.LeftPadding, AbsoluteGridLength(16)),
                    (Column.Card, StarGridLength(1)),
                    (Column.RightPadding, AbsoluteGridLength(16)));

                Children.Add(new CardViewFrame().Row(Row.Card).Column(Column.Card));

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            }

            enum Row { TopPadding, Card, BottomPadding }
            enum Column { LeftPadding, Card, RightPadding }

            class CardViewFrame : PancakeView
            {
                public CardViewFrame()
                {
                    CornerRadius = 4;
                    HasShadow = false;
                    Padding = new Thickness(16);
                    BorderThickness = 2;
                    Content = new ContentGrid();

                    SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                    SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
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
                        (Column.Site, StarGridLength(isSmallScreen() ? 2 : 5)),
                        (Column.SitePadding, AbsoluteGridLength(16)),
                        (Column.Referrals, StarGridLength(3)),
                        (Column.ReferralPadding, AbsoluteGridLength(separatorPadding)),
                        (Column.Separator, AbsoluteGridLength(1)),
                        (Column.UniquePadding, AbsoluteGridLength(separatorPadding)),
                        (Column.Uniques, StarGridLength(2)));

                    Children.Add(new FavIconImage()
                                        .Row(Row.Title).Column(Column.FavIcon).RowSpan(2));

                    Children.Add(new TitleLabel("SITE", TextAlignment.Start, LayoutOptions.Start)
                                        .Row(Row.Title).Column(Column.Site));

                    Children.Add(new DescriptionLabel()
                                        .Row(Row.Description).Column(Column.Site)
                                        .Bind(Label.TextProperty, nameof(MobileReferringSiteModel.Referrer)));

                    Children.Add(new TitleLabel("REFERRALS", TextAlignment.End, LayoutOptions.End).Assign(out TitleLabel referralsTitleLabel)
                                        .Row(Row.Title).Column(Column.Referrals));

                    Children.Add(new StatisticsLabel(referralsTitleLabel)
                                        .Row(Row.Description).Column(Column.Referrals)
                                        .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(MobileReferringSiteModel.TotalCount), convert: statisticsConverter));

                    Children.Add(new Separator()
                                        .Row(Row.Title).Column(Column.Separator).RowSpan(2));

                    Children.Add(new TitleLabel("UNIQUE", TextAlignment.Start, LayoutOptions.Start).Assign(out TitleLabel uniqueTitleLabel)
                                        .Row(Row.Title).Column(Column.Uniques));

                    Children.Add(new StatisticsLabel(uniqueTitleLabel)
                                        .Row(Row.Description).Column(Column.Uniques)
                                        .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(MobileReferringSiteModel.TotalUniqueCount), convert: statisticsConverter));

                    static string statisticsConverter(long number) => number.ConvertToAbbreviatedText();
                    static bool isSmallScreen() => (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density) <= 400;
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

                        HeightRequest = _favIconHeight;
                        WidthRequest = _favIconWidth;
                        CornerRadius = Math.Max(_favIconHeight, _favIconWidth) / 2;
                        HorizontalOptions = VerticalOptions = LayoutOptions.Start;
                        BackgroundColor = Color.White;

                        Padding = new Thickness(padding);

                        var circleImage = new CircleImage
                        {
                            Aspect = Aspect.AspectFit,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            HeightRequest = _favIconHeight - padding * 2,
                            WidthRequest = _favIconWidth - padding * 2,
                        };
                        circleImage.SetBinding(CircleImage.SourceProperty, nameof(MobileReferringSiteModel.FavIcon));

                        Content = circleImage;
                    }
                }
            }
        }
    }
}