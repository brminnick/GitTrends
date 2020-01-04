using System;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class ReferringSitesDataTemplate : DataTemplate
    {
        public ReferringSitesDataTemplate() : base(CreateGrid)
        {
        }

        static Grid CreateGrid()
        {
            var referrerLabel = new Label();
            referrerLabel.SetBinding(Label.TextProperty, nameof(ReferringSiteModel.Referrer));

            var viewsCountText = new Span();
            viewsCountText.SetBinding(Span.TextProperty, nameof(ReferringSiteModel.TotalCount));

            var uniqueViewsCountText = new Span();
            uniqueViewsCountText.SetBinding(Span.TextProperty, nameof(ReferringSiteModel.TotalUniqueCount));

            var viewsText = new FormattedString
            {
                Spans =
                {
                    new Span
                    {
                        Text = "Views: ",
                        FontAttributes = FontAttributes.Bold
                    },
                    viewsCountText
                }
            };

            var viewsLabel = new Label { FormattedText = viewsText };

            var uniqueViewsText = new FormattedString
            {
                Spans =
                {
                    new Span
                    {
                        Text = "Unique Views: ",
                        FontAttributes = FontAttributes.Bold
                    },
                    uniqueViewsCountText
                }
            };

            var uniqueViewsLabel = new Label { FormattedText = uniqueViewsText };

            var grid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = new GridLength(20, GridUnitType.Absolute) }
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                }
            };

            grid.Children.Add(referrerLabel, 0, 0);
            grid.Children.Add(viewsLabel, 1, 0);
            grid.Children.Add(uniqueViewsLabel, 2, 0);

            return grid;
        }
    }
}
