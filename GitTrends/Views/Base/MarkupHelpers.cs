using System;
using System.Linq.Expressions;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    static class ElementExtensions
    {
        public static TElement DynamicResource<TElement>(this TElement element, BindableProperty property, string key) where TElement : Element
        { element.SetDynamicResource(property, key); return element; }

        public static TElement DynamicResource<TElement>(this TElement element, params (BindableProperty property, string key)[] resources) where TElement : Element
        { foreach (var resource in resources) element.SetDynamicResource(resource.property, resource.key); return element; }
    }

    public static class MarkupHelpers
    {
        public static RelativeLayout RelativeLayout(params ConstrainedView[] constrainedViews)
        {
            var layout = new RelativeLayout();
            foreach (var constrainedView in constrainedViews) constrainedView?.AddTo(layout);
            return layout;
        }

        public static ConstrainedView UnConstrained<TView>(this TView view) where TView : View
        => ConstrainedView.FromView(view);

        public static ConstrainedView Constrain<TView>(this TView view, Expression<Func<Rectangle>> bounds) where TView : View
        => ConstrainedView.FromBounds(view, bounds);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public static ConstrainedView Constrain<TView>(this TView view, Expression<Func<double>> x = null, Expression<Func<double>> y = null, Expression<Func<double>> width = null, Expression<Func<double>> height = null) where TView : View
        => ConstrainedView.FromExpressions(view, x, y, width, height);

        public static ConstrainedView Constrain<TView>(this TView view, Constraint xConstraint = null, Constraint yConstraint = null, Constraint widthConstraint = null, Constraint heightConstraint = null) where TView : View
        => ConstrainedView.FromConstraints(view, xConstraint, yConstraint, widthConstraint, heightConstraint);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        public class ConstrainedView {
            enum Kind { None, Bounds, Expressions, Constraints }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            View view;
            Kind kind;
            Expression<Func<Rectangle>> bounds;
            Expression<Func<double>> x, y, width, height;
            Constraint xConstraint, yConstraint, widthConstraint, heightConstraint;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

            internal void AddTo(RelativeLayout layout)
            {
                switch (kind)
                {
                    case Kind.None:
                        ((Layout<View>)layout).Children.Add(view);
                        break;
                    case Kind.Bounds:
                        layout.Children.Add(view, bounds);
                        break;
                    case Kind.Expressions:
                        layout.Children.Add(view, x, y, width, height);
                        break;
                    case Kind.Constraints:
                        layout.Children.Add(view, xConstraint, yConstraint, widthConstraint, heightConstraint);
                        break;
                }
            }

            internal static ConstrainedView FromView(View view)
                => new ConstrainedView { view = view, kind = Kind.None };

            internal static ConstrainedView FromBounds(View view, Expression<Func<Rectangle>> bounds)
                => new ConstrainedView { view = view, kind = Kind.Bounds, bounds = bounds };

            internal static ConstrainedView FromExpressions(View view, Expression<Func<double>> x, Expression<Func<double>> y, Expression<Func<double>> width, Expression<Func<double>> height)
                => new ConstrainedView { view = view, kind = Kind.Expressions, x = x, y = y, width = width, height = height };

            internal static ConstrainedView FromConstraints(View view, Constraint xConstraint, Constraint yConstraint, Constraint widthConstraint, Constraint heightConstraint)
                => new ConstrainedView { view = view, kind = Kind.Constraints, xConstraint = xConstraint, yConstraint = yConstraint, widthConstraint = widthConstraint, heightConstraint = heightConstraint };
        }
    }
}