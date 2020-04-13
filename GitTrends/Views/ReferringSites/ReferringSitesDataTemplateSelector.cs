using System;
using System.Diagnostics;
using GitTrends.Mobile.Shared;
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
            public ReferringSitesDataTemplate(MobileReferringSiteModel referringSiteModel) : base(() => new CardView(referringSiteModel))
            {
            }

            class CardView : Grid
            {
                public CardView(in MobileReferringSiteModel referringSiteModel)
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

                    Children.Add(new CardViewFrame(referringSiteModel).Row(Row.Card).Column(Column.Card));

                    SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
                }

                enum Row { TopPadding, Card, BottomPadding }
                enum Column { LeftPadding, Card, RightPadding }

                class CardViewFrame : PancakeView
                {
                    public CardViewFrame(in MobileReferringSiteModel referringSiteModel)
                    {
                        CornerRadius = 4;
                        HasShadow = false;
                        Padding = new Thickness(16);
                        BorderThickness = 2;
                        Content = new ContentGrid(referringSiteModel);

                        SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                        SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
                    }
                }

                class ContentGrid : Grid
                {
                    const int _favIconWidth = MobileReferringSiteModel.FavIconSize;
                    const int _favIconHeight = MobileReferringSiteModel.FavIconSize;

                    public ContentGrid(in MobileReferringSiteModel referringSiteModel)
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

                        Children.Add(new FavIconImage(referringSiteModel.FavIcon)
                                            .Row(Row.Title).Column(Column.FavIcon).RowSpan(2));

                        Children.Add(new TitleLabel("SITE")
                                            .Row(Row.Title).Column(Column.Site));

                        Children.Add(new PrimaryColorLabel(referringSiteModel.Referrer)
                                            .Row(Row.Description).Column(Column.Site));

                        Children.Add(new TitleLabel("REFERRALS")
                                            .Row(Row.Title).Column(Column.Referrals).Center());

                        Children.Add(new StatisticsLabel(12, referringSiteModel.TotalCount, nameof(BaseTheme.PrimaryTextColor), FontFamilyConstants.RobotoRegular)
                                            .Row(Row.Description).Column(Column.Referrals).Center());

                        Children.Add(new Separator()
                                            .Row(Row.Title).Column(Column.Separator).RowSpan(2).FillExpandVertical());

                        Children.Add(new TitleLabel("UNIQUE")
                                            .Row(Row.Title).Column(Column.Uniques).Center());

                        Children.Add(new StatisticsLabel(12, referringSiteModel.TotalUniqueCount, nameof(BaseTheme.PrimaryTextColor), FontFamilyConstants.RobotoRegular)
                                            .Row(Row.Description).Column(Column.Uniques).Center());
                    }

                    enum Row { Title, Description }
                    enum Column { FavIcon, FavIconPadding, Site, SitePadding, Referrals, ReferralPadding, Separator, UniquePadding, Uniques }

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

                    class StatisticsLabel : Label
                    {
                        public StatisticsLabel(in double fontSize, in long number, in string textColorThemeName, in string fontFamily)
                        {
                            Text = number.ConvertToAbbreviatedText();
                            FontSize = fontSize;

                            HorizontalOptions = LayoutOptions.FillAndExpand;
                            MaxLines = 1;
                            HorizontalTextAlignment = TextAlignment.Start;
                            VerticalTextAlignment = TextAlignment.End;
                            LineBreakMode = LineBreakMode.TailTruncation;
                            FontFamily = fontFamily;

                            SetDynamicResource(TextColorProperty, textColorThemeName);
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
                            const int padding = 1;

                            HeightRequest = _favIconHeight;
                            WidthRequest = _favIconWidth;
                            CornerRadius = Math.Max(_favIconHeight, _favIconWidth) / 2;
                            HorizontalOptions = VerticalOptions = LayoutOptions.Start;
                            BackgroundColor = Color.White;

                            Padding = new Thickness(padding);

                            Debug.WriteLine($"FavIconImageSource: {imageSource}");

                            Content = new CircleImage
                            {
                                Aspect = Aspect.AspectFit,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                HeightRequest = _favIconHeight - padding * 2,
                                WidthRequest = _favIconWidth - padding * 2,
                                Source = imageSource
                            };
                        }
                    }
                }
            }
        }
    }
}