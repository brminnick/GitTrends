using GitTrends.Mobile.Common;
using GitTrends.Resources;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace GitTrends;

partial class MauiProgram
{
	static void CustomizeHandlers()
	{
		AddPickerBorderCustomHandler();
		AddSearchPageCustomHandler();
		AddSwitchCustomHandler();
	}
	
	static void AddSwitchCustomHandler()
	{
		SwitchHandler.Mapper.AppendToMapping("SwitchBackgroundColor", (handler, view) =>
		{
			var buttonBackgroundColor = AppResources.GetResource<Color>(nameof(BaseTheme.ButtonBackgroundColor));
			handler.PlatformView.TintColor = buttonBackgroundColor.MultiplyAlpha(0.25f).ToPlatform();
		});
	}

	static void AddPickerBorderCustomHandler()
	{
		PickerHandler.Mapper.AppendToMapping("PickerBorder", (handler, view) =>
		{
			ThemeService.PreferenceChanged += HandlePreferenceChanged;

			handler.PlatformView.TextAlignment = UIKit.UITextAlignment.Center;
			handler.PlatformView.Layer.BorderWidth = 1;
			handler.PlatformView.Layer.CornerRadius = 5;

			SetPickerBorder();

			void SetPickerBorder()
			{
				if (Application.Current?.Resources is BaseTheme theme)
				{
					var borderColor = Resources.AppResources.GetResource<Color>(nameof(BaseTheme.PickerBorderColor));
					handler.PlatformView.Layer.BorderColor = borderColor.ToCGColor();
				}
			}

			void HandlePreferenceChanged(object? sender, PreferredTheme e) => SetPickerBorder();
		});
	}

	static void AddSearchPageCustomHandler()
	{
		PageHandler.Mapper.AppendToMapping("SearchPage", (handler, view) =>
		{
			if (view is not (ISearchPage and ContentPage page))
				return;

			var searchController = new UISearchController(searchResultsController: null)
			{
				// SearchResultsUpdater = new SearchResultsUpdating(),
				ObscuresBackgroundDuringPresentation = false,
				HidesNavigationBarDuringPresentation = false,
				HidesBottomBarWhenPushed = true
			};
			searchController.SearchBar.Placeholder = string.Empty;

			page.Loaded += HandlePageLoaded;
			page.Unloaded += HandlePageUnloaded;
			page.SizeChanged += HandleSizeChanged;

			void HandlePageLoaded(object? sender, EventArgs e)
			{
				if (Platform.GetCurrentUIViewController() is ShellFlyoutRenderer { NavigationItem.SearchController: null } flyoutRenderer)
				{
					flyoutRenderer.NavigationItem.SearchController = searchController;
					flyoutRenderer.DefinesPresentationContext = true;

					//Work-around to ensure the SearchController appears when the page first appears https://stackoverflow.com/a/46313164/5953643
					flyoutRenderer.NavigationItem.SearchController.Active = true;
					flyoutRenderer.NavigationItem.SearchController.Active = false;
				}
			}

			void HandlePageUnloaded(object? sender, EventArgs e)
			{
				if (handler.PlatformView.GetNavigationController() is { ParentViewController.NavigationItem.SearchController: not null } navigationController)
				{
					navigationController.NavigationItem.SearchController = null;
				}
			}

			void HandleSizeChanged(object? sender, EventArgs e)
			{
				ArgumentNullException.ThrowIfNull(sender);

				var page = (Page)sender;

				if (Platform.GetCurrentUIViewController() is ShellFlyoutRenderer { NavigationItem.SearchController: not null } && page.Height > -1)
				{
					page.SizeChanged -= HandleSizeChanged;

					var statusBarSize = UIApplication.SharedApplication.StatusBarFrame.Size;
					var statusBarHeight = Math.Min(statusBarSize.Height, statusBarSize.Width);

					page.Padding = new Thickness(page.Padding.Left,
						page.Padding.Top,
						page.Padding.Right,
						page.Padding.Bottom + statusBarHeight);
				}
			}
		});
	}
}