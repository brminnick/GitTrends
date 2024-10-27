using System.Collections;

namespace GitTrends;

static class MauiService
{
	// **** Example Screen Sizes ****
	// iPhone SE 2nd Gen (aka iPhone 8), Width 750, Density 2, Width/Density 375
	// iPhone 11, Width 828, Density 2, Width/Density 414
	// Pixel 3, Width 1080, Density 2.75, Width/Density 392.7
	// Galaxy S5, Width 1080, Density 3, Width/Density 360
	// Galaxy Nexus, Width 720, Density 2, Width/Density 360
	public static double ScreenWidth { get; } = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
	public static double ScreenHeight { get; } = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
	public static bool IsSmallScreen { get; } = ScreenWidth <= 360;

	public static bool IsNullOrEmpty(this IEnumerable? enumerable) => !enumerable?.GetEnumerator().MoveNext() ?? true;

	public static void SetSelectedStateBackgroundColor(this VisualElement visualElement, Color color)
	{
		if (VisualStateManager.GetVisualStateGroups(visualElement).FirstOrDefault(static x => x.Name is nameof(VisualStateManager.CommonStates)) is VisualStateGroup commonStatesGroup
			&& commonStatesGroup.States.FirstOrDefault(static x => x.Name is VisualStateManager.CommonStates.Selected) is VisualState selectedVisualState
			&& selectedVisualState.Setters.FirstOrDefault(static x => x.Property == VisualElement.BackgroundColorProperty) is Setter backgroundColorPropertySetter)
		{
			backgroundColorPropertySetter.Value = color;
		}
		else
		{
			VisualStateManager.SetVisualStateGroups(visualElement,
			[
				new VisualStateGroup
				{
					Name = nameof(VisualStateManager.CommonStates),
					States =
					{
						new VisualState
						{
							Name = VisualStateManager.CommonStates.Selected,
							Setters =
							{
								new Setter
								{
									Property= VisualElement.BackgroundColorProperty,
									Value = color
								}
							}
						}
					}
				}
			]);
		}
	}

	public static void CompressAllLayouts(this Layout layout)
	{
		var childLayouts = GetChildLayouts(layout);

		foreach (var childLayout in childLayouts)
			CompressAllLayouts(childLayout);

		if (layout.BackgroundColor == default && !layout.GestureRecognizers.Any())
			CompressedLayout.SetIsHeadless(layout, true);
	}

	public static void CancelAllAnimations(in IView element)
	{
		switch (element)
		{
			case ContentView contentView:
				CancelAllAnimations(contentView.Content);
				break;

			case Layout layout:
				var childLayoutsOfLayout = GetChildLayouts(layout);

				foreach (var childLayout in childLayoutsOfLayout)
					CancelAllAnimations(childLayout);

				foreach (var view in layout.Children)
					CancelAllAnimations(view);
				break;

			case View view:
				Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(view);
				break;
		}
	}

	static IEnumerable<Layout> GetChildLayouts(in Layout layout)
	{
		if (layout.Children is null || !layout.Children.Any())
			return [];

		var childLayouts = layout.Children.OfType<Layout>();

		var childContentViews = layout.Children.OfType<ContentView>();
		var childContentViewLayouts = childContentViews.Select(static x => x.Content).OfType<Layout>();

		return childLayouts.Concat(childContentViewLayouts);
	}
}