using System.ComponentModel;
using System.Reflection;
using CoreGraphics;
using Foundation;
using GitTrends.Mobile.Common;
using GitTrends.Resources;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace GitTrends;

// Fix Thank you to https://github.com/vhugogarcia
sealed class ShellWithLargeTitlesHandler : ShellRenderer
{
	public static IShellPageRendererTracker? Tracker { get; set; }

	protected override IShellPageRendererTracker CreatePageRendererTracker()
	{
		if (Tracker is not null)
			throw new InvalidOperationException("This should have been cleared out by CustomShellSectionRenderer");

		return Tracker = new CustomShellPageRendererTracker(this);
	}

	protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
	{
		return new CustomShellSectionRenderer(this);
	}

	sealed class CustomShellSectionRootHeader(IShellContext shellContext) : ShellSectionRootHeader(shellContext)
	{
		volatile bool _isRotating;

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			if (_isRotating)
				Invoke(CollectionView.ReloadData, 0);

			_isRotating = false;
		}


		public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
		{
			base.ViewWillTransitionToSize(toSize, coordinator);
			_isRotating = true;
		}
	}

	sealed class CustomShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext, CustomShellSectionRenderer customShellSectionRenderer) : ShellSectionRootRenderer(shellSection, shellContext)
	{
		readonly ShellSection _shellSection = shellSection;

		UIEdgeInsets _additionalSafeArea = UIEdgeInsets.Zero;

		public CustomShellSectionRootHeader? CustomShellSectionRootHeader { get; private set; }

		IShellSectionController ShellSectionController => _shellSection;

		public override void AddChildViewController(UIViewController childController)
		{
			base.AddChildViewController(childController);
			UpdateAdditionalSafeAreaInsets(childController);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			UpdateAdditionalSafeAreaInsets();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			customShellSectionRenderer.SnagTracker();
		}

		protected override IShellSectionRootHeader CreateShellSectionRootHeader(IShellContext shellContext)
		{
			return CustomShellSectionRootHeader = new CustomShellSectionRootHeader(shellContext);
		}

		protected override void LayoutRenderers()
		{
			base.LayoutRenderers();
			UpdateAdditionalSafeAreaInsets();
		}

		void UpdateAdditionalSafeAreaInsets()
		{
			ShellSectionController.GetItems();

			foreach (var shellContent in ChildViewControllers)
			{
				UpdateAdditionalSafeAreaInsets(shellContent);
			}
		}

		void UpdateAdditionalSafeAreaInsets(UIViewController viewController)
		{
			if (viewController is not PageViewController)
				return;

			_additionalSafeArea = ShellSectionController.GetItems().Count > 1
				? new UIEdgeInsets(35, 0, 0, 0)
				: UIEdgeInsets.Zero;

			if (!viewController.AdditionalSafeAreaInsets.Equals(_additionalSafeArea))
				viewController.AdditionalSafeAreaInsets = _additionalSafeArea;
		}
	}

	sealed class CustomShellSectionRenderer : ShellSectionRenderer
	{
		readonly Dictionary<Element, IShellPageRendererTracker> _trackers = [];

		CustomShellSectionRootRenderer? _customShellSectionRootRenderer;

		public CustomShellSectionRenderer(IShellContext context) : base(context)
		{
			var navDelegate = (UINavigationControllerDelegate)Delegate;
			Delegate = new NavDelegate(navDelegate, this);
			Context = context;

			UpdateLargeTitleSettings();
			AddShadow();

			ThemeService.PreferenceChanged += HandlePreferenceChanged;
		}

		public IShellContext Context { get; }

		public void SnagTracker()
		{
			if (Tracker is null)
				return;

			_trackers[Tracker.Page] = Tracker;
			Tracker = null;
		}

		public override void PushViewController(UIViewController viewController, bool animated)
		{
			SnagTracker();
			base.PushViewController(viewController, animated);
			SnagTracker();
		}

		protected override void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.HandleShellPropertyChanged(sender, e);

			if (e.PropertyName == ShellAttachedProperties.PrefersLargeTitlesProperty.PropertyName)
				UpdateLargeTitleSettings();
		}

		protected override IShellSectionRootRenderer CreateShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext)
		{
			return _customShellSectionRootRenderer = new CustomShellSectionRootRenderer(shellSection, shellContext, this);
		}

		protected override void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			SnagTracker();
			base.OnNavigationRequested(sender, e);
			SnagTracker();
		}

		protected override void OnPushRequested(NavigationRequestedEventArgs e)
		{
			SnagTracker();
			base.OnPushRequested(e);
			SnagTracker();
		}

		protected override void OnInsertRequested(NavigationRequestedEventArgs e)
		{
			SnagTracker();
			base.OnInsertRequested(e);
			SnagTracker();
		}

		void HandlePreferenceChanged(object? sender, PreferredTheme e)
		{
			AddShadow();
		}

		void AddShadow()
		{
			var themeService = IPlatformApplication.Current?.Services.GetRequiredService<ThemeService>()
				?? throw new InvalidOperationException($"{nameof(ThemeService)} not found");

			if (themeService.IsLightTheme())
			{
				NavigationBar.Layer.ShadowColor = UIColor.Gray.CGColor;
				NavigationBar.Layer.ShadowOffset = new CGSize();
				NavigationBar.Layer.ShadowOpacity = 1;
			}
			else if (themeService.IsDarkTheme())
			{
				NavigationBar.Layer.ShadowColor = UIColor.White.CGColor;
				NavigationBar.Layer.ShadowOffset = new CGSize(0, -3);
				NavigationBar.Layer.ShadowOpacity = 0;
			}
		}

		void UpdateLargeTitleSettings()
		{
			var value = ShellAttachedProperties.GetPrefersLargeTitles(Shell.Current);
			ViewController.NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Always;
			NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Always;
			NavigationBar.PrefersLargeTitles = value;

			Invoke(() =>
			{
				if (_customShellSectionRootRenderer?.CustomShellSectionRootHeader is UICollectionViewController uvcvc)
				{
					uvcvc.CollectionView.ReloadData();
				}
			}, 0);
		}

		sealed class NavDelegate(UINavigationControllerDelegate navDelegate, CustomShellSectionRenderer customShellSectionRenderer) : UINavigationControllerDelegate
		{
			// This is currently working around a Mono Interpreter bug
			// if you remove this code please verify that hot restart still works
			// https://github.com/xamarin/Xamarin.Forms/issues/10519
			[Export("navigationController:animationControllerForOperation:fromViewController:toViewController:")]
			[Foundation.Preserve(Conditional = true)]
			public new IUIViewControllerAnimatedTransitioning? GetAnimationControllerForOperation(
				UINavigationController navigationController,
				UINavigationControllerOperation operation,
				UIViewController fromViewController,
				UIViewController toViewController)
			{
				return null;
			}

			public override void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				navDelegate.DidShowViewController(navigationController, viewController, animated);
			}

			public override void WillShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				navDelegate.WillShowViewController(navigationController, viewController, animated);

				// Because the back button title needs to be set on the previous VC
				// We want to set the BackButtonItem as early as possible so there is no flickering
				var currentPage = customShellSectionRenderer.Context.Shell?.CurrentPage;
				var trackers = customShellSectionRenderer._trackers;
				if (currentPage?.Handler is IPlatformViewHandler pvh &&
					ReferenceEquals(pvh.ViewController, viewController) &&
					trackers.TryGetValue(currentPage, out var tracker) &&
					tracker is CustomShellPageRendererTracker shellRendererTracker)
				{
					shellRendererTracker.UpdateToolbarItemsInternal(false);
				}
			}
		}
	}

	sealed class CustomShellPageRendererTracker : ShellPageRendererTracker
	{
		public CustomShellPageRendererTracker(IShellContext context) : base(context)
		{
			Context = context;
			ThemeService.PreferenceChanged += HandleThemePreferenceChanged;
		}

		public IShellContext Context { get; }

		Page? ToolbarCurrentPage
		{
			get
			{
				var toolBar = ((IToolbarElement)Context.Shell).Toolbar;
				var toolbarType = toolBar?.GetType();
				var currentPageProperty = toolbarType?.GetField("_currentPage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
				var result = (Page?)currentPageProperty?.GetValue(toolBar);
				return result;

			}
		}

		internal void UpdateToolbarItemsInternal(bool updateWhenLoaded = true)
		{
			if (updateWhenLoaded && Page.IsLoaded || !updateWhenLoaded)
				UpdateToolbarItems();
		}

		protected override async void UpdateToolbarItems()
		{
			base.UpdateToolbarItems();

			if (ViewController?.NavigationItem is null)
			{
				return;
			}

			UpdateBackButtonTitle();
			await UpdateBarButtonItems(ViewController, Page);
		}

		protected override void UpdateTitle()
		{
			if (!IsToolbarReady())
				return;

			base.UpdateTitle();
		}

		protected override async void UpdateTitleView()
		{
			if (!IsToolbarReady())
				return;

			if (ViewController?.NavigationItem is null)
			{
				return;
			}

			if (Page is ISearchPage && Page.Handler is not null)
			{
				var searchController = new UISearchController(searchResultsController: null)
				{
					ObscuresBackgroundDuringPresentation = false,
					HidesNavigationBarDuringPresentation = false,
					HidesBottomBarWhenPushed = true
				};
				searchController.SearchBar.BarStyle = UIBarStyle.Black;
				searchController.SearchBar.Placeholder = string.Empty;
				searchController.SearchBar.TextChanged += HandleTextChanged;

				ViewController.NavigationItem.SearchController = searchController;
				ViewController.DefinesPresentationContext = true;

				//Work-around to ensure the SearchController appears when the page first appears https://stackoverflow.com/a/46313164/5953643
				ViewController.NavigationItem.SearchController.Active = true;
				ViewController.NavigationItem.SearchController.Active = false;
			}

			var titleView = Shell.GetTitleView(Page) ?? Shell.GetTitleView(Context.Shell);
			if (titleView is null)
			{
				var view = ViewController.NavigationItem.TitleView;
				ViewController.NavigationItem.TitleView = null;
				view?.Dispose();
			}
			else
			{
				var view = new CustomTitleViewContainer(titleView);
				ViewController.NavigationItem.TitleView = view;
			}

			UpdateNavigationBarColors();
			await UpdateBarButtonItems(ViewController, Page);
		}

		static async Task<UIBarButtonItem> GetUIBarButtonItem(ToolbarItem toolbarItem)
		{
			var image = await GetUIImage(toolbarItem.IconImageSource);

			return image switch
			{
				null => new UIBarButtonItem(toolbarItem.Text, UIBarButtonItemStyle.Plain, ExecuteBarButtonItem)
				{
					AccessibilityIdentifier = toolbarItem.AutomationId
				},
				_ => new UIBarButtonItem(image, UIBarButtonItemStyle.Plain, ExecuteBarButtonItem)
				{
					AccessibilityIdentifier = toolbarItem.AutomationId
				}
			};

			void ExecuteBarButtonItem(object? sender, EventArgs e)
			{
				if (toolbarItem.Command?.CanExecute(toolbarItem.CommandParameter) is true)
				{
					toolbarItem.Command.Execute(toolbarItem.CommandParameter);
				}
			}
		}

		static async Task<UIImage?> GetUIImage(ImageSource source)
		{
			var imageSourceServiceResult = source switch
			{
				FileImageSource => await new FileImageSourceService().GetImageAsync(source),
				UriImageSource => await new UriImageSourceService().GetImageAsync(source),
				StreamImageSource => await new StreamImageSourceService().GetImageAsync(source),
				_ => throw new NotSupportedException($"{source.GetType().FullName} is not yet supported")
			};

			return imageSourceServiceResult?.Value;
		}

		void UpdateNavigationBarColors()
		{
			if (AppResources.TryGetResource<Color>(nameof(BaseTheme.NavigationBarTextColor), out var barTextColor)
				&& AppResources.TryGetResource<Color>(nameof(BaseTheme.NavigationBarBackgroundColor), out var barBackgroundColor)
				&& barTextColor is not null
				&& barBackgroundColor is not null)
			{
				var titleAttributes = new UIStringAttributes
				{
					BackgroundColor = barBackgroundColor.ToPlatform(),
					ForegroundColor = barTextColor.ToPlatform()
				};

				var appearance = new UINavigationBarAppearance
				{
					TitleTextAttributes = titleAttributes,
					LargeTitleTextAttributes = titleAttributes,
					BackgroundColor = barBackgroundColor.ToPlatform()
				};

				ViewController.NavigationItem.CompactAppearance = appearance;
				ViewController.NavigationItem.StandardAppearance = appearance;
				ViewController.NavigationItem.ScrollEdgeAppearance = appearance;
				ViewController.NavigationItem.CompactScrollEdgeAppearance = appearance;
			}
		}

		void HandleTextChanged(object? sender, UISearchBarTextChangedEventArgs e)
		{
			var searchPage = (ISearchPage)Page;
			searchPage.OnSearchBarTextChanged(e.SearchText);
		}

		async Task UpdateBarButtonItems(UIViewController parentViewController, Page page)
		{
			var (leftBarButtonItems, rightBarButtonItems) = await GetToolbarItems(page.ToolbarItems);

			parentViewController.NavigationItem.LeftBarButtonItems = leftBarButtonItems?.Any() switch
			{
				true => [.. leftBarButtonItems],
				false or null => []
			};

			parentViewController.NavigationItem.RightBarButtonItems = rightBarButtonItems.Any() switch
			{
				true => [.. rightBarButtonItems],
				false => []
			};
		}

		static async Task<(IReadOnlyList<UIBarButtonItem>? LeftBarButtonItem, IReadOnlyList<UIBarButtonItem> RightBarButtonItems)> GetToolbarItems(IList<ToolbarItem> items)
		{
			var leftBarButtonItems = new List<UIBarButtonItem>();
			foreach (var item in items.Where(static x => x.Priority is 1))
			{
				var barButtonItem = await GetUIBarButtonItem(item);
				leftBarButtonItems.Add(barButtonItem);
			}

			var rightBarButtonItems = new List<UIBarButtonItem>();
			foreach (var item in items.Where(static x => x.Priority != 1))
			{
				var barButtonItem = await GetUIBarButtonItem(item);
				rightBarButtonItems.Add(barButtonItem);
			}

			return (leftBarButtonItems, rightBarButtonItems);
		}

		bool IsToolbarReady()
		{
			return ToolbarCurrentPage == Page;
		}

		void UpdateBackButtonTitle()
		{
			var behavior = Shell.GetBackButtonBehavior(Page);
			var text = behavior.GetPropertyIfSet<string?>(BackButtonBehavior.TextOverrideProperty, null);

			var navController = ViewController?.NavigationController;

			if (navController is not null)
			{
				var viewControllers = navController.ViewControllers ?? throw new InvalidOperationException($"{nameof(ViewController.NavigationController.ViewControllers)} cannot be null");
				var count = viewControllers.Length;

				if (count > 1 && ReferenceEquals(viewControllers[count - 1], ViewController))
				{
					var previousNavItem = viewControllers[count - 2].NavigationItem;
					if (!string.IsNullOrWhiteSpace(text))
					{
						var barButtonItem = previousNavItem.BackBarButtonItem ??= new UIBarButtonItem();
						barButtonItem.Title = text;
					}
					else if (previousNavItem.BackBarButtonItem is not null)
					{
						previousNavItem.BackBarButtonItem = null;
					}
				}
			}
		}

		async void HandleThemePreferenceChanged(object? sender, PreferredTheme e)
		{
			UpdateNavigationBarColors();
			await UpdateBarButtonItems(ViewController, Page);
		}
	}

	sealed class CustomTitleViewContainer : UIContainerView
	{
		public CustomTitleViewContainer(View view) : base(view)
		{
			MatchHeight = true;
			TranslatesAutoresizingMaskIntoConstraints = false;
		}

		public override CGRect Frame
		{
			get => base.Frame;
			set
			{
				if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
				    && Superview is not null)
				{
					value.Y = Superview.Bounds.Y;
					value.Height = Superview.Bounds.Height;
				}

				base.Frame = value;
			}
		}

		public override CGSize IntrinsicContentSize => UILayoutFittingExpandedSize;

		public override CGSize SizeThatFits(CGSize size) => size;

		public override void LayoutSubviews()
		{
			if (Height is null or 0
			    && Superview is not null)
			{
				UpdateFrame(Superview);
			}

			base.LayoutSubviews();
		}

		public override void WillMoveToSuperview(UIView? newSuperView)
		{
			if (newSuperView is not null)
				UpdateFrame(newSuperView);

			base.WillMoveToSuperview(newSuperView);
		}

		void UpdateFrame(UIView newSuperView)
		{
			if (newSuperView.Bounds != CGRect.Empty)
			{
				if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)))
					Frame = new CGRect(Frame.X, newSuperView.Bounds.Y, Frame.Width, newSuperView.Bounds.Height);

				Height = newSuperView.Bounds.Height;
			}
		}
	}

	abstract class UIContainerView : UIView
	{
		IPlatformViewHandler? _renderer;
		UIView? _platformView;
		bool _disposed;
		double _measuredHeight;

		protected UIContainerView(View view)
		{
			View = view;

			UpdatePlatformView();
			ClipsToBounds = true;
			MeasuredHeight = double.NaN;
			Margin = new Thickness(0);
		}

		public virtual Thickness Margin { get; }

		internal event EventHandler? HeaderSizeChanged;

		internal View View { get; }

		internal bool MatchHeight { get; set; }

		internal double MeasuredHeight
		{
			get
			{
				if (MatchHeight && Height is not null)
					return Height.Value;

				return _measuredHeight;
			}

			private set => _measuredHeight = value;
		}

		internal double? Height { get; set; }

		internal double? Width { get; set; }

		public override CGSize SizeThatFits(CGSize size)
		{
			var measuredSize = (View as IView).Measure(size.Width, size.Height);

			if (Height is not null && MatchHeight)
			{
				MeasuredHeight = Height.Value;
			}
			else
			{
				MeasuredHeight = measuredSize.Height;
			}

			return new CGSize(size.Width, MeasuredHeight);
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			Disconnect();
			base.WillRemoveSubview(uiview);
		}

		public override void LayoutSubviews()
		{
			if (!IsPlatformViewValid())
				return;

			var height = Height ?? MeasuredHeight;
			var width = Width ?? Frame.Width;

			if (double.IsNaN(height))
				return;

			var platformFrame = new Rect(0, 0, width, height);


			if (MatchHeight)
				((IView)View).Measure(width, height);

			((IView)View).Arrange(platformFrame);
		}

		internal void Disconnect()
		{
		}

		internal void UpdatePlatformView()
		{
			if (GetMauiContext(View) is IMauiContext mauiContext)
			{
				_renderer = View.ToHandler(mauiContext);
				_platformView = _renderer.ContainerView ?? _renderer.PlatformView;

				if (_platformView is not null && !ReferenceEquals(_platformView.Superview, this))
					AddSubview(_platformView);
			}
		}

		private protected void OnHeaderSizeChanged()
		{
			HeaderSizeChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				Disconnect();

				if (ReferenceEquals(_platformView?.Superview, this))
					_platformView.RemoveFromSuperview();

				_renderer = null;
				_platformView = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		bool IsPlatformViewValid()
		{
			if (_platformView?.Superview is null || _renderer is null)
				return false;

			return _platformView.Superview.Equals(this);
		}

		static IMauiContext? GetMauiContext(in Element? element, bool fallbackToAppMauiContext = false)
		{
			if (element is IElement { Handler.MauiContext: not null } mauiCoreElement)
				return mauiCoreElement.Handler.MauiContext;

			foreach (var parent in GetParentsPath(element))
			{
				if (parent is IElement { Handler.MauiContext: not null } parentView)
					return parentView.Handler.MauiContext;
			}

			return fallbackToAppMauiContext ? GetMauiContext(Application.Current) : default;
		}

		static IEnumerable<Element> GetParentsPath(Element? self)
		{
			var current = self;

			while (current?.RealParent is not ((not null) or IApplication))
			{
				if (current is not null)
				{
					yield return current;

					current = current.RealParent;
				}
			}
		}
	}
}

public static class ShellAttachedProperties
{
	public static readonly BindableProperty PrefersLargeTitlesProperty =
		BindableProperty.Create("PrefersLargeTitles", typeof(bool), typeof(ShellAttachedProperties), false);

	public static bool GetPrefersLargeTitles(BindableObject element) => (bool)element.GetValue(PrefersLargeTitlesProperty);

	public static void SetPrefersLargeTitles(BindableObject element, bool value)
	{
		element.SetValue(PrefersLargeTitlesProperty, value);
	}

	public static IPlatformElementConfiguration<iOS, Shell> SetPrefersLargeTitles(this IPlatformElementConfiguration<iOS, Shell> config, bool value)
	{
		SetPrefersLargeTitles(config.Element, value);
		return config;
	}

	public static bool PrefersLargeTitles(this IPlatformElementConfiguration<iOS, Shell> config)
	{
		return GetPrefersLargeTitles(config.Element);
	}
}