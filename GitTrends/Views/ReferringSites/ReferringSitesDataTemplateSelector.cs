using System;
using FFImageLoading.Forms;
using Xamarin.Forms;

namespace GitTrends
{
    class ReferringSitesDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => new ReferringSitesDataTemplate((ReferringSiteModel)item);

        class ReferringSitesDataTemplate : DataTemplate
        {
            public ReferringSitesDataTemplate(ReferringSiteModel referringSiteModel) : base(() => CreateReferringSitesDataTemplate(referringSiteModel))
            {
            }

            static View CreateReferringSitesDataTemplate(in ReferringSiteModel referringSiteModel)
            {
                const int rowHeight = ReferringSiteModel.FavIconSize + 10;

                var favIcon = new CachedImage
                {
                    HeightRequest = ReferringSiteModel.FavIconSize,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    LoadingPlaceholder = FavIconService.DefaultFavIcon,
                    ErrorPlaceholder = FavIconService.DefaultFavIcon,
                    Source = referringSiteModel.FavIcon,
                };

                var urlTitleLabel = new TitleLabel("Referring Site");

                var urlDescriptionLabel = new DescriptionLabel(referringSiteModel.Referrer)
                {
                    LineBreakMode = LineBreakMode.TailTruncation
                };

                var viewsTitleLabel = new TitleLabel("Views");
                var viewsDescriptionLabel = new DescriptionLabel(referringSiteModel.TotalCount.ToString());

                var uniqueViewsTitleLabel = new TitleLabel("Uniqe Visitors");
                var uniqueViewsDescriptionLabel = new DescriptionLabel(referringSiteModel.TotalUniqueCount.ToString());

                var grid = new Grid
                {
                    RowSpacing = 1,
                    Margin = new Thickness(5),
                    RowDefinitions =
                    {
                        new RowDefinition { Height = new GridLength(2, GridUnitType.Absolute) },
                        new RowDefinition { Height = new GridLength(rowHeight / 2, GridUnitType.Absolute) },
                        new RowDefinition { Height = new GridLength(rowHeight / 2, GridUnitType.Absolute) },
                        new RowDefinition { Height = new GridLength(2, GridUnitType.Absolute) },
                    },
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(5, GridUnitType.Absolute) },
                        new ColumnDefinition { Width = new GridLength(rowHeight, GridUnitType.Absolute) },
                        new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(5, GridUnitType.Absolute) },
                    }
                };

                grid.Children.Add(favIcon, 1, 0);
                Grid.SetRowSpan(favIcon, 4);

                grid.Children.Add(urlTitleLabel, 2, 1);
                grid.Children.Add(urlDescriptionLabel, 2, 2);

                grid.Children.Add(viewsTitleLabel, 3, 1);
                grid.Children.Add(viewsDescriptionLabel, 3, 2);

                grid.Children.Add(uniqueViewsTitleLabel, 4, 1);
                grid.Children.Add(uniqueViewsDescriptionLabel, 4, 2);

                return grid;
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
