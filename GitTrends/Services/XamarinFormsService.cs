using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace GitTrends
{
    public static class XamarinFormsService
    {
        public static GridLength StarGridLength(double value) => new GridLength(value, GridUnitType.Star);
        public static GridLength AbsoluteGridLength(double value) => new GridLength(value, GridUnitType.Absolute);

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

