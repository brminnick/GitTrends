using FFImageLoading.Svg.Forms;
using GitTrends.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace GitTrends
{
    class RepositoryDataTemplate : DataTemplate
    {
        const int _smallFontSize = 12;

        public RepositoryDataTemplate() : base(CreateRepositoryDataTemplate)
        {
        }

        static Grid CreateRepositoryDataTemplate()
        {
            const int circleImageHeight = 84;
            const int emojiColumnSize = 15;
            const int countColumnSize = 30;

            var image = new CircleImage
            {
                HeightRequest = circleImageHeight,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
            };
            image.SetBinding(CircleImage.SourceProperty, nameof(Repository.OwnerAvatarUrl));

            var repositoryNameLabel = new DarkBlueLabel
            {
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Start,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            repositoryNameLabel.SetBinding(Label.TextProperty, nameof(Repository.Name));

            var repositoryDescriptionLabel = new DarkBlueLabel
            {
                FontSize = _smallFontSize,
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalTextAlignment = TextAlignment.Start,
                FontAttributes = FontAttributes.Italic
            };
            repositoryDescriptionLabel.SetBinding(Label.TextProperty, nameof(Repository.Description));

            var starsSVGImage = new SmallNavyBlueSVGImage("star.svg");
            var starsCountLabel = new DarkBlueLabel(_smallFontSize - 1);
            starsCountLabel.SetBinding(Label.TextProperty, nameof(Repository.StarCount));

            var forksSVGImage = new SmallNavyBlueSVGImage("repo_forked.svg");
            var forksCountLabel = new DarkBlueLabel(_smallFontSize - 1);
            forksCountLabel.SetBinding(Label.TextProperty, nameof(Repository.ForkCount));

            var issuesSVGImage = new SmallNavyBlueSVGImage("issue_opened.svg");
            var issuesCountLabel = new DarkBlueLabel(_smallFontSize - 1);
            issuesCountLabel.SetBinding(Label.TextProperty, nameof(Repository.IssuesCount));

            var grid = new Grid
            {
                BackgroundColor = Color.Transparent,

                Padding = new Thickness(2, 0, 0, 5),
                RowSpacing = 2,
                ColumnSpacing = 3,

                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.StartAndExpand,

                RowDefinitions = {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) },
                    new RowDefinition { Height = new GridLength(20, GridUnitType.Absolute) },
                    new RowDefinition { Height = new GridLength(45, GridUnitType.Absolute) },
                    new RowDefinition { Height = new GridLength(_smallFontSize + 2, GridUnitType.Absolute) },
                    new RowDefinition { Height = new GridLength(5, GridUnitType.Absolute) },
                },
                ColumnDefinitions = {
                    new ColumnDefinition { Width = new GridLength(circleImageHeight, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(emojiColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(countColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(emojiColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(countColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(emojiColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(countColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };

            grid.Children.Add(image, 0, 0);
            Grid.SetRowSpan(image, 5);

            grid.Children.Add(repositoryNameLabel, 2, 1);
            Grid.SetColumnSpan(repositoryNameLabel, 7);

            grid.Children.Add(repositoryDescriptionLabel, 2, 2);
            Grid.SetColumnSpan(repositoryDescriptionLabel, 7);

            grid.Children.Add(starsSVGImage, 2, 3);
            grid.Children.Add(starsCountLabel, 3, 3);

            grid.Children.Add(forksSVGImage, 4, 3);
            grid.Children.Add(forksCountLabel, 5, 3);

            grid.Children.Add(issuesSVGImage, 6, 3);
            grid.Children.Add(issuesCountLabel, 7, 3);

            return grid;
        }

        class SmallNavyBlueSVGImage : SvgCachedImage
        {
            public SmallNavyBlueSVGImage(in string svgFileName)
            {
                var app = (App)Application.Current;
                app.ThemeChanged += HandleThemeChanged;

                UpdateSVGColor();

                Source = SvgService.GetSVGResourcePath(svgFileName);
                HeightRequest = _smallFontSize;
            }

            void HandleThemeChanged(object sender, Theme e) => UpdateSVGColor();

            void UpdateSVGColor()
            {
                var textColor = (Color)Application.Current.Resources[nameof(BaseTheme.TextColor)];
                ReplaceStringMap = SvgService.GetColorStringMap(textColor.ToHex());
            }
        }

        class DarkBlueLabel : Label
        {
            public DarkBlueLabel(in double fontSize) : this() => FontSize = fontSize;

            public DarkBlueLabel()
            {
                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.End;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
            }
        }
    }
}
