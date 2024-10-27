using _Microsoft.Android.Resource.Designer;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.Widget;
using GitTrends.Mobile.Common;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace GitTrends;

partial class MauiProgram
{
	static void CustomizeHandlers()
	{
		AddNoVerticalScrollBarCustomHandler();
		AddLowercaseButtonCustomHandler();
		AddPickerBorderCustomHandler();
		AddSearchPageCustomHandler();
	}

	static void AddPickerBorderCustomHandler()
	{
		PickerHandler.Mapper.AppendToMapping("PickerBorder", (handler, view) =>
		{
			ThemeService.PreferenceChanged += HandlePreferenceChanged;

			handler.PlatformView.Background = null;
			handler.PlatformView.Gravity = GravityFlags.Center;
			handler.PlatformView.VerticalScrollBarEnabled = false;

			SetPickerBorder();

			void SetPickerBorder()
			{
				if (Application.Current?.Resources is BaseTheme theme)
				{
					var borderColor = theme.PickerBorderColor;

					var gradientDrawable = new GradientDrawable();
					gradientDrawable.SetCornerRadius(10);
					gradientDrawable.SetStroke(2, borderColor.ToPlatform());

					try
					{
						handler.PlatformView.Background = gradientDrawable;
					}
					catch (ObjectDisposedException)
					{

					}
				}
			}

			void HandlePreferenceChanged(object? sender, PreferredTheme e) => SetPickerBorder();
		});
	}

	static void AddNoVerticalScrollBarCustomHandler()
	{
		LabelHandler.Mapper.AppendToMapping("NoVerticalScrollBar", (handler, view) =>
		{
			handler.PlatformView.VerticalScrollBarEnabled = false;
		});
	}

	static void AddLowercaseButtonCustomHandler()
	{
		ButtonHandler.Mapper.AppendToMapping("LowercaseButton", (handler, view) =>
		{
			handler.PlatformView.SetAllCaps(false);
			handler.PlatformView.VerticalScrollBarEnabled = false;
		});
	}

	static void AddSearchPageCustomHandler()
	{
		PageHandler.Mapper.AppendToMapping("SearchPage", (handler, view) =>
		{
			Shell.Current.Navigated += HandleShellNavigated;

			if (view is ISearchPage and Page page)
				page.SizeChanged += HandlePageSizeChanged;

			static IEnumerable<MaterialToolbar> GetToolbars(ViewGroup viewGroup)
			{
				for (var i = 0; i < viewGroup.ChildCount; i++)
				{
					if (viewGroup.GetChildAt(i) is MaterialToolbar toolbar)
					{
						yield return toolbar;
					}
					else if (viewGroup.GetChildAt(i) is ViewGroup childViewGroup)
					{
						foreach (var childToolbar in GetToolbars(childViewGroup))
							yield return childToolbar;
					}
				}
			}

			void AddSearchToToolbar(string pageTitle)
			{
				if (GetToolbar() is MaterialToolbar toolBar
					&& toolBar.Menu?.FindItem(ResourceConstant.Id.ActionSearch)?.ActionView?.GetType() != typeof(SearchView))
				{
					toolBar.Title = pageTitle;
					toolBar.InflateMenu(ResourceConstant.Menu.MainMenu);

					if (toolBar.Menu?.FindItem(ResourceConstant.Id.ActionSearch)?.ActionView is SearchView searchView)
					{
						searchView.QueryTextChange += HandleQueryTextChange;
					}
				}
			}

			MaterialToolbar? GetToolbar()
			{
				if (Platform.CurrentActivity?.Window?.DecorView.RootView is ViewGroup viewGroup)
				{
					var toolbars = GetToolbars(viewGroup);

					//Return top-most Toolbar
					return toolbars.LastOrDefault();
				}

				return null;
			}

			void HandleQueryTextChange(object? sender, SearchView.QueryTextChangeEventArgs e)
			{
				if (view is ISearchPage searchPage)
					searchPage.OnSearchBarTextChanged(e.NewText ?? string.Empty);
			}

			//Workaround to re-add the SearchView when navigating back to an ISearchPage, because MAUI automatically removes it
			void HandleShellNavigated(object? sender, ShellNavigatedEventArgs e)
			{
				if (sender is AppShell { CurrentPage: ISearchPage and Page currentPage })
					AddSearchToToolbar(currentPage.Title ?? string.Empty);
			}

			void HandlePageSizeChanged(object? sender, EventArgs e)
			{
				if (sender is ISearchPage and Page searchPage)
					AddSearchToToolbar(searchPage.Title ?? string.Empty);
			}
		});
	}
}