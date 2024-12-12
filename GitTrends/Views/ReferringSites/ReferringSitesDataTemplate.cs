using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Sharpnado.MaterialFrame;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

class ReferringSitesDataTemplate() : DataTemplate(() => new CardView())
{
	public const int TopPadding = 12;
	public const int BottomPadding = 4;

	sealed class CardView : Grid
	{
		public CardView()
		{
			RowSpacing = 0;
			RowDefinitions = Rows.Define(
				(Row.TopPadding, TopPadding),
				(Row.Card, Star),
				(Row.BottomPadding, BottomPadding));

			ColumnDefinitions = Columns.Define(
				(Column.LeftPadding, 16),
				(Column.Card, Star),
				(Column.RightPadding, 16));

			Children.Add(new CardViewFrame().Row(Row.Card).Column(Column.Card));

			this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
		}

		enum Row { TopPadding, Card, BottomPadding }
		enum Column { LeftPadding, Card, RightPadding }

		sealed class CardViewFrame : MaterialFrame
		{
			public CardViewFrame()
			{
				CornerRadius = 4;
				HasShadow = true;
				Padding = new Thickness(16);
				Elevation = 4;
				Content = new ContentGrid();

				this.DynamicResource(MaterialThemeProperty, nameof(BaseTheme.MaterialFrameTheme));
			}
		}

		sealed class ContentGrid : Grid
		{
			const int _favIconWidth = MobileReferringSiteModel.FavIconSize;
			const int _favIconHeight = MobileReferringSiteModel.FavIconSize;

			public ContentGrid()
			{
				const int rowSpacing = 6;
				const int separatorPadding = 12;

				HorizontalOptions = LayoutOptions.Fill;
				VerticalOptions = LayoutOptions.Fill;

				RowSpacing = rowSpacing;
				ColumnSpacing = 0;

				RowDefinitions = Rows.Define(
					(Row.Title, _favIconHeight / 2 - rowSpacing / 2),
					(Row.Description, _favIconHeight / 2 - rowSpacing / 2));

				ColumnDefinitions = Columns.Define(
					(Column.FavIcon, _favIconWidth),
					(Column.FavIconPadding, 16),
					(Column.Site, Star),
					(Column.SitePadding, 8),
					(Column.Referrals, Auto),
					(Column.ReferralPadding, separatorPadding),
					(Column.Separator, 1),
					(Column.UniquePadding, separatorPadding),
					(Column.Uniques, Auto));

				Children.Add(new FavIconImage()
					.Row(Row.Title).Column(Column.FavIcon).RowSpan(2));

				Children.Add(new TitleLabel(ReferringSitesPageConstants.Site, TextAlignment.Start, LayoutOptions.Start)
					.Row(Row.Title).Column(Column.Site));

				Children.Add(new DescriptionLabel()
					.Row(Row.Description).Column(Column.Site)
					.Bind(Label.TextProperty,
						getter: static (MobileReferringSiteModel vm) => vm.Referrer,
						mode: BindingMode.OneTime));

				Children.Add(new TitleLabel(ReferringSitesPageConstants.Referrals, TextAlignment.End, LayoutOptions.End).Assign(out TitleLabel referralsTitleLabel)
					.Row(Row.Title).Column(Column.Referrals));

				Children.Add(new StatisticsLabel(referralsTitleLabel)
					.Row(Row.Description).Column(Column.Referrals)
					.Bind(Label.TextProperty,
						getter: static (MobileReferringSiteModel vm) => vm.TotalCount,
						mode: BindingMode.OneTime,
						convert: static (long count) => count.ToAbbreviatedText()));

				Children.Add(new Separator()
					.Row(Row.Title).Column(Column.Separator).RowSpan(2));

				Children.Add(new TitleLabel(ReferringSitesPageConstants.Unique, TextAlignment.Start, LayoutOptions.Start).Assign(out TitleLabel uniqueTitleLabel)
					.Row(Row.Title).Column(Column.Uniques));

				Children.Add(new StatisticsLabel(uniqueTitleLabel)
					.Row(Row.Description).Column(Column.Uniques)
					.Bind(Label.TextProperty,
						getter: static (MobileReferringSiteModel vm) => vm.TotalUniqueCount,
						mode: BindingMode.OneTime,
						convert: static (long count) => count.ToAbbreviatedText()));
			}

			enum Row { Title, Description }
			enum Column { FavIcon, FavIconPadding, Site, SitePadding, Referrals, ReferralPadding, Separator, UniquePadding, Uniques }

			sealed class TitleLabel : Label
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

					this.DynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
				}
			}

			sealed class StatisticsLabel : Label
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

					this.DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
					this.Bind(WidthRequestProperty, 
						getter: label => label.Width, 
						source: titleLabel);
				}
			}

			sealed class DescriptionLabel : Label
			{
				public DescriptionLabel()
				{
					MaxLines = 1;
					FontSize = 12;
					FontFamily = FontFamilyConstants.RobotoRegular;
					LineBreakMode = LineBreakMode.TailTruncation;

					HorizontalOptions = LayoutOptions.Fill;
					HorizontalTextAlignment = TextAlignment.Start;

					VerticalTextAlignment = TextAlignment.End;

					this.DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
				}
			}

			sealed class Separator : BoxView
			{
				public Separator()
				{
					VerticalOptions = LayoutOptions.Fill;
					this.DynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
				}
			}

			sealed class FavIconImage : CircleBorder
			{
				public FavIconImage()
				{
					const int padding = 1;

					this.Start();

					BackgroundColor = Colors.White;

					Padding = new Thickness(padding);
					HeightRequest = WidthRequest = Math.Min(_favIconHeight, _favIconWidth);

					Content = new CircleImage
					{
						GetBorderColor = () => Colors.Transparent,
						ErrorPlaceholder = FavIconService.DefaultFavIcon,
						LoadingPlaceholder = FavIconService.DefaultFavIcon
					}.Bind(CircleImage.ImageSourceProperty, 
						getter: static (MobileReferringSiteModel vm) => vm.FavIcon, 
						mode: BindingMode.OneWay);
				}
			}
		}
	}
}