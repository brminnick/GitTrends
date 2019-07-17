using GitTrends.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace GitTrends
{
    class RepositoryViewCell : ViewCell
    {
        public const int ImageHeight = 105;

        const int _smallFontSize = 12;
        const int _repositoryDetailColumnSize = 50;

        const string _starEmoji = "⭐️";
        const string _tuningForkEmoji = "⑂";
        const string _antEmoji = "🐜";

        readonly CircleImage _image;
        readonly Label _repositoryNameLabel;
        readonly Label _repositoryDescriptionLabel;
        readonly Label _starsLabel;
        readonly Label _forksLabel;
        readonly Label _issuesLabel;

        public RepositoryViewCell()
        {
            _image = new CircleImage
            {
                HeightRequest = ImageHeight,
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
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Start,
                FontAttributes = FontAttributes.Italic
            };

            _starsLabel = new DarkBlueLabel
            {
                HorizontalTextAlignment = TextAlignment.Start,
                FontSize = _smallFontSize,
            };

            _forksLabel = new DarkBlueLabel
            {
                HorizontalTextAlignment = TextAlignment.Start,
                FontSize = _smallFontSize,
            };

            _issuesLabel = new DarkBlueLabel
            {
                HorizontalTextAlignment = TextAlignment.Start,
                FontSize = _smallFontSize,
            };


            var grid = new Grid
            {
                BackgroundColor = Color.Transparent,

                Margin = new Thickness(5, 5, 5, 0),
                RowSpacing = 2,

                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.StartAndExpand,

                RowDefinitions = {
                    new RowDefinition { Height = new GridLength(20, GridUnitType.Absolute) },
                    new RowDefinition { Height = new GridLength(45, GridUnitType.Absolute) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                },
                ColumnDefinitions = {
                    new ColumnDefinition { Width = new GridLength(ImageHeight * 4/5, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(_repositoryDetailColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(_repositoryDetailColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(_repositoryDetailColumnSize, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };

            grid.Children.Add(_image, 0, 0);
            Grid.SetRowSpan(_image, 3);

            grid.Children.Add(_repositoryNameLabel, 1, 0);
            Grid.SetColumnSpan(_repositoryNameLabel, 4);

            grid.Children.Add(_repositoryDescriptionLabel, 1, 1);
            Grid.SetColumnSpan(_repositoryDescriptionLabel, 4);

            grid.Children.Add(_starsLabel, 1, 2);
            grid.Children.Add(_forksLabel, 2, 2);
            grid.Children.Add(_issuesLabel, 3, 2);

            View = grid;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            _image.Source = null;
            _repositoryNameLabel.Text = null;
            _repositoryDescriptionLabel.Text = null;
            _starsLabel.Text = null;
            _forksLabel.Text = null;
            _issuesLabel.Text = null;

            if (BindingContext is Repository repository)
            {
                _image.Source = repository.OwnerAvatarUrl;
                _repositoryNameLabel.Text = repository.Name;
                _repositoryDescriptionLabel.Text = repository.Description;
                _starsLabel.Text = $"{_starEmoji} {repository.StarCount}";
                _forksLabel.Text = $"{_tuningForkEmoji} {repository.ForkCount}";
                _issuesLabel.Text = $"{_antEmoji} {repository.IssuesCount}";
            }
        }

        class DarkBlueLabel : Label
        {
            public DarkBlueLabel() => TextColor = ColorConstants.DarkBlue;
        }
    }
}
