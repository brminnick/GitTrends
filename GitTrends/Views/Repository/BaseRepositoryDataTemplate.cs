using System;
using System.Collections.Generic;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Sharpnado.MaterialFrame;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static GitTrends.XamarinFormsService;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
	abstract class BaseRepositoryDataTemplate : DataTemplate
	{
		public const int TopPadding = 12;
		public const int BottomPadding = 4;

		const int _statsColumnSize = 40;
		const double _statisticsRowHeight = StatisticsLabel.StatisticsFontSize + 4;
		const double _emojiColumnSize = _statisticsRowHeight;

		readonly static double _circleImageHeight = IsSmallScreen ? 48 : 62;

		readonly static AsyncAwaitBestPractices.WeakEventManager _tappedWeakEventManager = new();

		protected BaseRepositoryDataTemplate(Func<object> loadTemplate) : base(loadTemplate)
		{

		}

		public static event EventHandler Tapped
		{
			add => _tappedWeakEventManager.AddEventHandler(value);
			remove => _tappedWeakEventManager.RemoveEventHandler(value);
		}

		protected enum Row { Title, Description, DescriptionPadding, Separator, SeparatorPadding, Statistics }
		protected enum Column { Avatar, AvatarPadding, Trending, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3 }

		protected class CardView : ExtendedSwipeView<Repository>
		{
			public CardView(in IEnumerable<View> dataTemplateChildren)
			{
				var sidePadding = IsSmallScreen ? 8 : 16;
				BackgroundColor = Color.Transparent;

				Tapped += HandleTapped;

				RightItems = new SwipeItems
				{
					new SwipeItemView
					{
						Content = new SvgImage(() => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)], 44, 44)
										.Margins(right: sidePadding)
										.Bind(SvgImage.SourceProperty,
												nameof(Repository.IsFavorite),
												BindingMode.OneTime,
												convert: static (bool? isFavorite) => isFavorite is true
																						? SvgService.GetValidatedFullPath("star.svg")
																						: SvgService.GetValidatedFullPath("star_outline.svg"))

					}.Bind(SwipeItemView.CommandProperty, nameof(RepositoryViewModel.ToggleIsFavoriteCommand), BindingMode.OneTime, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(RepositoryViewModel)))
					 .Bind(SwipeItemView.CommandParameterProperty, mode: BindingMode.OneTime)
				};

				LeftItems = new SwipeItems
				{
					new SwipeItemView
					{
						Content = new Label { Text = FontAwesomeConstants.ExternalLink.ToString() }
										.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
										.Font(FontFamilyConstants.FontAwesome, 28).CenterExpand()
										.Margins(left: sidePadding),

					}.Bind(SwipeItemView.CommandProperty, nameof(RepositoryViewModel.NavigateToRepositoryWebsiteCommand), BindingMode.OneTime, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(RepositoryViewModel)))
					 .Bind(SwipeItemView.CommandParameterProperty, mode: BindingMode.OneTime)
				};

				Content = new Grid
				{
					RowSpacing = 0,

					RowDefinitions = Rows.Define(
						(CardViewRow.TopPadding, TopPadding),
						(CardViewRow.Card, Star),
						(CardViewRow.BottomPadding, BottomPadding)),

					ColumnDefinitions = Columns.Define(
						(CardViewColumn.LeftPadding, sidePadding),
						(CardViewColumn.Card, Star),
						(CardViewColumn.RightPadding, sidePadding)),

					Children =
					{
						new CardViewFrame(dataTemplateChildren).Row(CardViewRow.Card).Column(CardViewColumn.Card)
					}
				};
			}

			enum CardViewRow { TopPadding, Card, BottomPadding }
			enum CardViewColumn { LeftPadding, Card, RightPadding }

			void HandleTapped(object sender, EventArgs e) => _tappedWeakEventManager.RaiseEvent(this, e, nameof(BaseRepositoryDataTemplate.Tapped));

			class CardViewFrame : MaterialFrame
			{
				public CardViewFrame(in IEnumerable<View> dataTemplateChildren)
				{
					Padding = IsSmallScreen ? new Thickness(8, 16, 6, 8) : new Thickness(16, 16, 12, 8);
					CornerRadius = 4;
					HasShadow = false;
					Elevation = 4;

					Content = new ContentGrid(dataTemplateChildren);

					this.DynamicResource(MaterialThemeProperty, nameof(BaseTheme.MaterialFrameTheme));
				}

				class ContentGrid : Grid
				{
					public ContentGrid(in IEnumerable<View> dataTemplateChildren)
					{
						this.FillExpand();

						RowDefinitions = Rows.Define(
							(Row.Title, 25),
							(Row.Description, 40),
							(Row.DescriptionPadding, 4),
							(Row.Separator, 1),
							(Row.SeparatorPadding, 4),
							(Row.Statistics, _statisticsRowHeight));

						ColumnDefinitions = Columns.Define(
							(Column.Avatar, _circleImageHeight),
							(Column.AvatarPadding, IsSmallScreen ? 4 : 8),
							(Column.Trending, Star),
							(Column.Emoji1, _emojiColumnSize),
							(Column.Statistic1, _statsColumnSize),
							(Column.Emoji2, _emojiColumnSize),
							(Column.Statistic2, _statsColumnSize),
							(Column.Emoji3, _emojiColumnSize),
							(Column.Statistic3, _statsColumnSize));

						Children.Add(new AvatarImage(_circleImageHeight)
										.Row(Row.Title).Column(Column.Avatar).RowSpan(2)
										.Bind(AvatarImage.ImageSourceProperty, nameof(Repository.OwnerAvatarUrl), BindingMode.OneTime)
										.DynamicResources((CircleImage.BorderColorProperty, nameof(BaseTheme.SeparatorColor)),
															(CircleImage.ErrorPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource)),
															(CircleImage.LoadingPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource))));

						Children.Add(new NameLabel()
										.Row(Row.Title).Column(Column.Trending).ColumnSpan(7)
										.Bind(Label.TextProperty, nameof(Repository.Name), BindingMode.OneTime));

						Children.Add(new DescriptionLabel()
										.Row(Row.Description).Column(Column.Trending).ColumnSpan(7)
										.Bind(Label.TextProperty, nameof(Repository.Description)));

						Children.Add(new Separator()
										.Row(Row.Separator).Column(Column.Trending).ColumnSpan(7));

						//On large screens, display TrendingImage in the same column as the repository name
						Children.Add(new TrendingImage(RepositoryPageAutomationIds.LargeScreenTrendingImage)
										.Row(Row.SeparatorPadding).Column(Column.Trending).RowSpan(2)
										.Assign(out TrendingImage largeScreenTrendingImage)
										.Bind(IsVisibleProperty,
												binding1: new Binding(nameof(Repository.IsTrending), BindingMode.OneWay),
												binding2: new Binding(nameof(Width), BindingMode.OneWay, source: this),
												binding3: new Binding(nameof(Repository.IsFavorite), BindingMode.OneWay),
												convert: ((bool IsTrending, double Width, bool IsFavorite) inputs) => IsTrendingImageVisible(inputs.IsTrending, inputs.Width, inputs.IsFavorite, largeScreenTrendingImageWidth => largeScreenTrendingImageWidth < (TrendingImage.SvgWidthRequest + 8))));

						//On smaller screens, display TrendingImage under the Avatar
						Children.Add(new TrendingImage(RepositoryPageAutomationIds.SmallScreenTrendingImage)
										.Row(Row.SeparatorPadding).Column(Column.Avatar).RowSpan(2).ColumnSpan(3)
										.Bind(IsVisibleProperty,
												binding1: new Binding(nameof(Repository.IsTrending), BindingMode.OneWay),
												binding2: new Binding(nameof(Width), BindingMode.OneWay, source: largeScreenTrendingImage),
												binding3: new Binding(nameof(Repository.IsFavorite), BindingMode.OneWay),
												convert: ((bool IsTrending, double Width, bool IsFavorite) inputs) => IsTrendingImageVisible(inputs.IsTrending, inputs.Width, inputs.IsFavorite, largeScreenTrendingImageWidth => largeScreenTrendingImageWidth >= (TrendingImage.SvgWidthRequest + 8))));

						foreach (var child in dataTemplateChildren)
						{
							Children.Add(child);
						}
					}

					class NameLabel : PrimaryColorLabel
					{
						public NameLabel() : base(20)
						{
							LineBreakMode = LineBreakMode.TailTruncation;
							HorizontalOptions = LayoutOptions.FillAndExpand;
							FontFamily = FontFamilyConstants.RobotoBold;
						}
					}

					class DescriptionLabel : PrimaryColorLabel
					{
						public DescriptionLabel() : base(14)
						{
							MaxLines = 2;
							LineHeight = 1.16;
							FontFamily = FontFamilyConstants.RobotoRegular;
						}
					}

					abstract class PrimaryColorLabel : Label
					{
						protected PrimaryColorLabel(in double fontSize)
						{
							FontSize = fontSize;
							LineBreakMode = LineBreakMode.TailTruncation;
							HorizontalTextAlignment = TextAlignment.Start;
							VerticalTextAlignment = TextAlignment.Start;

							this.DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
						}
					}

					class Separator : BoxView
					{
						public Separator() => this.DynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
					}

					class TrendingImage : SvgImage
					{
						public const double SvgWidthRequest = 62;
						public const double SvgHeightRequest = 16;

						public TrendingImage(string automationId) : base(SvgWidthRequest, SvgHeightRequest)
						{
							AutomationId = automationId;
							HorizontalOptions = LayoutOptions.Start;
							VerticalOptions = LayoutOptions.End;

							this.Bind(SvgImage.SourceProperty,
										nameof(Repository.IsTrending),
										BindingMode.OneTime,
										convert: static (bool isTrending) => isTrending ? SvgService.GetValidatedFullPath("trending_tag.svg") : SvgService.GetValidatedFullPath("favorite_tag.svg"));

							this.Bind<SvgImage, bool?, Func<Color>>(SvgImage.GetColorProperty,
																	nameof(Repository.IsFavorite),
																	BindingMode.OneTime,
																	convert: static (bool? isFavorite) => isFavorite is true
																									? () => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)]
																									: () => (Color)Application.Current.Resources[nameof(BaseTheme.CardTrendingStatsColor)]);
						}
					}

					static bool IsTrendingImageVisible(bool isTrending, double width, bool isFavorite, Func<double, bool> isWidthValid)
					{
						// When `Width is -1`, Xamarin.Forms hasn't inflated the View
						// Allow Xamarin.Forms to inflate the view, then validate its Width
						return (isTrending || isFavorite is true)
								&& (width is -1 || isWidthValid(width));
					}
				}
			}
		}
	}
}