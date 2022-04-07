using System;
using System.Collections;
using Xamarin.Forms;
using Bounds = System.Linq.Expressions.Expression<System.Func<Xamarin.Forms.Rectangle>>;
using Expression = System.Linq.Expressions.Expression<System.Func<double>>;

namespace GitTrends
{
	static class MarkupExtensions
	{
		public static double GetWidth(this View view, in RelativeLayout parent) => view.Measure(parent.Width, parent.Height).Request.Width;
		public static double GetHeight(this View view, in RelativeLayout parent) => view.Measure(parent.Width, parent.Height).Request.Height;

		public static bool IsNullOrEmpty(this IEnumerable? enumerable) => !enumerable?.GetEnumerator().MoveNext() ?? true;

		public static RelativeLayout RelativeLayout(params ConstrainedView?[] constrainedViews)
		{
			var layout = new RelativeLayout();
			foreach (var constrainedView in constrainedViews)
				constrainedView?.AddTo(layout);

			return layout;
		}

		public static ConstrainedView Unconstrained<TView>(this TView view) where TView : View => ConstrainedView.FromView(view);

		public static ConstrainedView Constrain<TView>(this TView view, Bounds bounds) where TView : View => ConstrainedView.FromBounds(view, bounds);

		public static ConstrainedView Constrain<TView>(this TView view, Expression? x = null, Expression? y = null, Expression? width = null, Expression? height = null) where TView : View => ConstrainedView.FromExpressions(view, x, y, width, height);

		public static ConstrainedView Constrain<TView>(this TView view, Constraint? xConstraint = null, Constraint? yConstraint = null, Constraint? widthConstraint = null, Constraint? heightConstraint = null) where TView : View => ConstrainedView.FromConstraints(view, xConstraint, yConstraint, widthConstraint, heightConstraint);

		public static RelativeLayout Add<TView>(this RelativeLayout relativeLayout, TView view) where TView : View?
		{
			if (view != null)
				((Layout<View>)relativeLayout).Children.Add(view);

			return relativeLayout;
		}

		public static RelativeLayout Add<TView>(this RelativeLayout relativeLayout, TView view, Bounds bounds) where TView : View?
		{
			if (view != null)
				relativeLayout.Children.Add(view, bounds);

			return relativeLayout;
		}

		public static RelativeLayout Add<TView>(this RelativeLayout relativeLayout, TView view, Expression? x = null, Expression? y = null, Expression? width = null, Expression? height = null) where TView : View?
		{
			if (view != null)
				relativeLayout.Children.Add(view, x, y, width, height);

			return relativeLayout;
		}

		public static RelativeLayout Add<TView>(this RelativeLayout relativeLayout, TView view, Constraint? xConstraint = null, Constraint? yConstraint = null, Constraint? widthConstraint = null, Constraint? heightConstraint = null) where TView : View?
		{
			if (view != null)
				relativeLayout.Children.Add(view, xConstraint, yConstraint, widthConstraint, heightConstraint);

			return relativeLayout;
		}
	}

	class ConstrainedView
	{
		readonly View _view;
		readonly Kind _kind;
		readonly Bounds? _bounds;
		readonly Expression? _x, _y, _width, _height;
		readonly Constraint? _xConstraint, _yConstraint, _widthConstraint, _heightConstraint;

		ConstrainedView(Kind kind, View view) => (_kind, _view) = (kind, view);

		ConstrainedView(Kind kind, View view, Bounds bounds) : this(kind, view) => _bounds = bounds;

		ConstrainedView(Kind kind, View view, Expression? x, Expression? y, Expression? width, Expression? height)
			: this(kind, view) => (_x, _y, _width, _height) = (x, y, width, height);

		ConstrainedView(Kind kind, View view, Constraint? xConstraint, Constraint? yConstraint, Constraint? widthConstraint, Constraint? heightConstraint)
			: this(kind, view) => (_xConstraint, _yConstraint, _widthConstraint, _heightConstraint) = (xConstraint, yConstraint, widthConstraint, heightConstraint);

		enum Kind { None, Bounds, Expressions, Constraints }

		internal void AddTo(RelativeLayout layout)
		{
			switch (_kind)
			{
				case Kind.None:
					((Layout<View>)layout).Children.Add(_view);
					break;

				case Kind.Bounds:
					layout.Children.Add(_view, _bounds);
					break;

				case Kind.Expressions:
					layout.Children.Add(_view, _x, _y, _width, _height);
					break;

				case Kind.Constraints:
					layout.Children.Add(_view, _xConstraint, _yConstraint, _widthConstraint, _heightConstraint);
					break;

				default:
					throw new NotSupportedException();
			}
		}

		internal static ConstrainedView FromView(View view) => new ConstrainedView(Kind.None, view);

		internal static ConstrainedView FromBounds(View view, Bounds bounds) => new ConstrainedView(Kind.Bounds, view, bounds);

		internal static ConstrainedView FromExpressions(View view, Expression? x, Expression? y, Expression? width, Expression? height) => new ConstrainedView(Kind.Expressions, view, x, y, width, height);

		internal static ConstrainedView FromConstraints(View view, Constraint? xConstraint, Constraint? yConstraint, Constraint? widthConstraint, Constraint? heightConstraint) => new ConstrainedView(Kind.Constraints, view, xConstraint, yConstraint, widthConstraint, heightConstraint);
	}
}