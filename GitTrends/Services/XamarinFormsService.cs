using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    static class XamarinFormsService
    {
        // **** Example Screen Sizes ****
        // iPhone SE 2nd Gen (aka iPhone 8), Width 750, Density 2, Width/Density 375
        // iPhone 11, Width 828, Density 2, Width/Density 414
        // Pixel 3, Width 1080, Density 2.75, Width/Density 392.7
        // Galaxy S5, Width 1080, Density 3, Width/Density 360
        // Galaxy Nexus, Width 720, Density 2, Width/Density 360
        public static double ScreenWidth { get; } = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        public static double ScreenHeight { get; } = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

        public static void SetSelectedStateBackgroundColor(this VisualElement visualElement, Color color)
        {
            if (VisualStateManager.GetVisualStateGroups(visualElement).FirstOrDefault(x => x.Name is nameof(VisualStateManager.CommonStates)) is VisualStateGroup commonStatesGroup
                && commonStatesGroup.States.FirstOrDefault(x => x.Name is VisualStateManager.CommonStates.Selected) is VisualState selectedVisualState
                && selectedVisualState.Setters.FirstOrDefault(x => x.Property == VisualElement.BackgroundColorProperty) is Setter backgroundColorPropertySetter)
            {
                backgroundColorPropertySetter.Value = color;
            }
            else
            {
                VisualStateManager.SetVisualStateGroups(visualElement, new VisualStateGroupList
                {
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
                });
            }
        }

        public static void CompressAllLayouts(this Layout<View> layout)
        {
            var childLayouts = GetChildLayouts(layout);

            foreach (var childLayout in childLayouts)
                CompressAllLayouts(childLayout);

            if (layout.BackgroundColor == default && !layout.GestureRecognizers.Any())
                CompressedLayout.SetIsHeadless(layout, true);
        }

        public static void CancelAllAnimations(in VisualElement element)
        {
            switch (element)
            {
                case ContentView contentView:
                    CancelAllAnimations(contentView.Content);
                    break;

                case Layout<View> layout:
                    var childLayoutsOfLayout = GetChildLayouts(layout);

                    foreach (var childLayout in childLayoutsOfLayout)
                        CancelAllAnimations(childLayout);

                    foreach (var view in layout.Children)
                        CancelAllAnimations(view);
                    break;

                case View view:
                    ViewExtensions.CancelAnimations(view);
                    break;
            }
        }

        static IEnumerable<Layout<View>> GetChildLayouts(in Layout<View> layout)
        {
            if (layout.Children is null || !layout.Children.Any())
                return Enumerable.Empty<Layout<View>>();

            var childLayouts = layout.Children.OfType<Layout<View>>();

            var childContentViews = layout.Children.OfType<ContentView>();
            var childContentViewLayouts = childContentViews.Select(x => x.Content).OfType<Layout<View>>();

            return childLayouts.Concat(childContentViewLayouts);
        }
    }
}

