using FFImageLoading.Svg.Forms;
using GitTrends.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace GitTrends
{
    class RepositoryViewCell : ViewCell
    {
        public const int RowHeight = 105;

        const int _smallFontSize = 12;
        const int _emojiColumnSize = 15;
        const int _countColumnSize = 30;

        readonly CircleImage _image;
        readonly SmallNavyBlueSVGImage _forksSVGImage, _starsSVGImage, _issuesSVGImage;
        readonly DarkBlueLabel _repositoryNameLabel, _repositoryDescriptionLabel, _starsCountLabel, _forksCountLabel, _issuesCountLabel;

        public RepositoryViewCell()
        {
            _image = new CircleImage
            {
                HeightRequest = RowHeight,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
            };

            _repositoryNameLabel = new DarkBlueLabel
            {
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Start,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            _repositoryDescriptionLabel = new DarkBlueLabel
            {
                FontSize = _smallFontSize,
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalTextAlignment = TextAlignment.Start,
                FontAttributes = FontAttributes.Italic
            };

            _starsSVGImage = new SmallNavyBlueSVGImage("star.svg");
            _starsCountLabel = new DarkBlueLabel(_smallFontSize);

            _forksSVGImage = new SmallNavyBlueSVGImage("repo_forked.svg");
            _forksCountLabel = new DarkBlueLabel(_smallFontSize);

            _issuesSVGImage = new SmallNavyBlueSVGImage("issue_opened.svg");
            _issuesCountLabel = new DarkBlueLabel(_smallFontSize);

            var grid = new Grid
            {
                BackgroundColor = Color.Transparent,

                Margin = new Thickness(10, 10, 10, 0),
                RowSpacing = 2,
                ColumnSpacing = 3,

                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.StartAndExpand,

                RowDefinitions = {
                    new RowDefinition { Height = new GridLength(20, GridUnitType.Absolute) },
                    new RowDefinition { Height = new GridLength(45, GridUnitType.Absolute) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                },
                ColumnDefinitions = {
                    new ColumnDefinition { Width = new GridLength(RowHeight * 4/5, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(_emojiColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(_countColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(_emojiColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(_countColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(_emojiColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(_countColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }                    
                }
            };

            grid.Children.Add(_image, 0, 0);
            Grid.SetRowSpan(_image, 3);

            grid.Children.Add(_repositoryNameLabel, 2, 0);
            Grid.SetColumnSpan(_repositoryNameLabel, 7);

            grid.Children.Add(_repositoryDescriptionLabel, 2, 1);
            Grid.SetColumnSpan(_repositoryDescriptionLabel, 7);

            grid.Children.Add(_starsSVGImage, 2, 2);
            grid.Children.Add(_starsCountLabel, 3, 2);

            grid.Children.Add(_forksSVGImage, 4, 2);
            grid.Children.Add(_forksCountLabel, 5, 2);

            grid.Children.Add(_issuesSVGImage, 6, 2);
            grid.Children.Add(_issuesCountLabel, 7, 2);

            View = grid;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            _image.Source = null;
            _repositoryNameLabel.Text = null;
            _repositoryDescriptionLabel.Text = null;
            _starsCountLabel.Text = null;
            _forksCountLabel.Text = null;
            _issuesCountLabel.Text = null;

            if (BindingContext is Repository repository)
            {
                _image.Source = repository.OwnerAvatarUrl;
                _repositoryNameLabel.Text = repository.Name;
                _repositoryDescriptionLabel.Text = repository.Description;
                _starsCountLabel.Text = repository.StarCount.ToString();
                _forksCountLabel.Text = repository.ForkCount.ToString();
                _issuesCountLabel.Text = repository.IssuesCount.ToString();
            }
        }

        class SmallNavyBlueSVGImage : SvgCachedImage
        {
            public SmallNavyBlueSVGImage(string svgFileName)
            {
                ReplaceStringMap = SvgService.GetColorStringMap(ColorConstants.LightNavyBlueHex);
                Source = SvgService.GetSVGResourcePath(svgFileName);
                HeightRequest = _smallFontSize;
            }
        }

        class DarkBlueLabel : Label
        {
            public DarkBlueLabel(double fontSize) : this() => FontSize = fontSize;

            public DarkBlueLabel()
            {
                TextColor = ColorConstants.DarkBlue;
                HorizontalTextAlignment = TextAlignment.Start;
            }
        }
    }
}
