using Xamarin.Forms;

namespace GitTrends
{
    static class MarkupHelpers
    {
        public static TElement DynamicResource<TElement>(this TElement element, BindableProperty property, string key) where TElement : Element
        { element.SetDynamicResource(property, key); return element; }

        public static TElement DynamicResource<TElement>(this TElement element, params (BindableProperty property, string key)[] resources) where TElement : Element
        { foreach (var resource in resources) element.SetDynamicResource(resource.property, resource.key); return element; }
    }
}
