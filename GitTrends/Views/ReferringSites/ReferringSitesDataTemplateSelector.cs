using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class ReferringSitesDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => new ReferringSitesDataTemplate((MobileReferringSiteModel)item);

        class ReferringSitesDataTemplate : DataTemplate
        {
            const int _favIconWidth = MobileReferringSiteModel.FavIconSize;
            const int _favIconHeight = MobileReferringSiteModel.FavIconSize;

            public ReferringSitesDataTemplate(MobileReferringSiteModel referringSiteModel) : base(() => CreateReferringSitesDataTemplate(referringSiteModel))
            {
            }

            enum Row { Title, Description }
            enum Column { FavIcon, FavIconPadding, Site, SitePadding, Referrals, ReferralPadding, Separator, UniquePadding, Uniques }

            static CardView CreateReferringSitesDataTemplate(in MobileReferringSiteModel referringSiteModel) => new CardView(CreateViews(referringSiteModel));

            static IEnumerable<View> CreateViews(in MobileReferringSiteModel referringSiteModel) => new View[]
            {
                new FavIconImage(referringSiteModel.FavIcon).Row(Row.Title).Column(Column.FavIcon).RowSpan(2),
                new TitleLabel("SITE").Row(Row.Title).Column(Column.Site),
                new PrimaryColorLabel(referringSiteModel.Referrer).Row(Row.Description).Column(Column.Site),
                new TitleLabel("REFERRALS").Row(Row.Title).Column(Column.Referrals).Center(),
                new StatisticsLabel(12, referringSiteModel.TotalCount, nameof(BaseTheme.PrimaryTextColor), FontFamilyConstants.RobotoRegular).Row(Row.Description).Column(Column.Referrals).Center(),
                new Separator().Row(Row.Title).Column(Column.Separator).RowSpan(2).FillExpandVertical(),
                new TitleLabel("UNIQUE").Row(Row.Title).Column(Column.Uniques).Center(),
                new StatisticsLabel(12, referringSiteModel.TotalUniqueCount, nameof(BaseTheme.PrimaryTextColor), FontFamilyConstants.RobotoRegular).Row(Row.Description).Column(Column.Uniques).Center()
            };

            class CardView : Grid
            {
                public CardView(in IEnumerable<View> children)
                {
                    RowSpacing = 0;
                    RowDefinitions = Rows.Define(
                        (CardViewRow.TopPadding, AbsoluteGridLength(8)),
                        (CardViewRow.Card, StarGridLength(1)),
                        (CardViewRow.BottomPadding, AbsoluteGridLength(8)));

                    ColumnDefinitions = Columns.Define(
                        (CardViewColumn.LeftPadding, AbsoluteGridLength(16)),
                        (CardViewColumn.Card, StarGridLength(1)),
                        (CardViewColumn.RightPadding, AbsoluteGridLength(16)));

                    Children.Add(new CardViewFrame(children).Row(CardViewRow.Card).Column(CardViewColumn.Card));

                    SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
                }

                enum CardViewRow { TopPadding, Card, BottomPadding }
                enum CardViewColumn { LeftPadding, Card, RightPadding }

                class CardViewFrame : PancakeView
                {
                    public CardViewFrame(in IEnumerable<View> children)
                    {
                        CornerRadius = 4;
                        HasShadow = false;
                        Padding = new Thickness(16);
                        BorderThickness = 2;
                        Content = new ContentGrid(children);

                        SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                        SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
                    }
                }

                class ContentGrid : Grid
                {
                    public ContentGrid(in IEnumerable<View> children)
                    {
                        const int rowSpacing = 6;

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
                            (Column.Site, StarGridLength(2)),
                            (Column.SitePadding, AbsoluteGridLength(16)),
                            (Column.Referrals, StarGridLength(1.5)),
                            (Column.ReferralPadding, AbsoluteGridLength(4)),
                            (Column.Separator, AbsoluteGridLength(1)),
                            (Column.UniquePadding, AbsoluteGridLength(4)),
                            (Column.Uniques, StarGridLength(1)));

                        foreach (var child in children)
                        {
                            Children.Add(child);
                        }
                    }
                }
            }

            class TitleLabel : Label
            {

                public TitleLabel(in string text)
                {
                    Text = text;
                    MaxLines = 1;
                    FontSize = 12;
                    CharacterSpacing = 1.56;
                    HorizontalOptions = LayoutOptions.Start;
                    HorizontalTextAlignment = TextAlignment.Start;
                    VerticalOptions = LayoutOptions.Start;
                    VerticalTextAlignment = TextAlignment.Start;
                    FontFamily = FontFamilyConstants.RobotoMedium;
                    LineBreakMode = LineBreakMode.TailTruncation;

                    SetDynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
                }
            }

            class PrimaryColorLabel : Label
            {
                public PrimaryColorLabel(in string text)
                {
                    Text = text;
                    MaxLines = 1;
                    FontSize = 12;
                    HorizontalTextAlignment = TextAlignment.Start;
                    HorizontalOptions = LayoutOptions.Start;
                    VerticalTextAlignment = TextAlignment.End;
                    FontFamily = FontFamilyConstants.RobotoRegular;
                    LineBreakMode = LineBreakMode.TailTruncation;

                    SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
                }
            }

            class Separator : BoxView
            {
                public Separator()
                {
                    SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
                }
            }

            class FavIconImage : PancakeView
            {
                public FavIconImage(in ImageSource? imageSource)
                {
                    HeightRequest = _favIconHeight;
                    WidthRequest = _favIconWidth;
                    CornerRadius = Math.Max(_favIconHeight, _favIconWidth) / 2;
                    HorizontalOptions = VerticalOptions = LayoutOptions.Start;
                    BackgroundColor = Color.White;

                    Padding = new Thickness(1);

                    Debug.WriteLine($"FavIconImageSource: {imageSource}");

                    Content = new CircleImage
                    {
                        Source = imageSource
                    };
                }
            }
        }
    }
}