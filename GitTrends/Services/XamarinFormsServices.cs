using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;

namespace GitTrends
{
    public static class XamarinFormsServices
    {
        #region Methods
        public static Task<T> BeginInvokeOnMainThreadAsync<T>(Func<Task<T>> method, Action onComplete = null)
        {
            var tcs = new TaskCompletionSource<T>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var task = method.Invoke();
                    tcs.SetResult(await task.ConfigureAwait(false));
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
                finally
                {
                    onComplete?.Invoke();
                }
            });

            return tcs.Task;
        }

        public static Task<T> BeginInvokeOnMainThreadAsync<T>(Func<T> method, Action onComplete = null)
        {
            var tcs = new TaskCompletionSource<T>();

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var result = method.Invoke();
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
                finally
                {
                    onComplete?.Invoke();
                }
            });

            return tcs.Task;
        }

        public static Task BeginInvokeOnMainThreadAsync(Action method, Action onComplete = null)
        {
            var tcs = new TaskCompletionSource<object>();

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    method.Invoke();
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
                finally
                {
                    onComplete?.Invoke();
                }
            });

            return tcs.Task;
        }

        public static Task BeginInvokeOnMainThreadAsync(Func<Task> method, Action onComplete = null)
        {
            var tcs = new TaskCompletionSource<object>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var task = method.Invoke();
                    await task.ConfigureAwait(false);

                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs?.SetException(e);
                }
                finally
                {
                    onComplete?.Invoke();
                }
            });

            return tcs?.Task;
        }

        public static void CompressAllLayouts(this Layout<View> layout)
        {
            var childLayouts = GetChildLayouts(layout);

            foreach (var childLayout in childLayouts)
                CompressAllLayouts(childLayout);

            if (layout.BackgroundColor == default && !layout.GestureRecognizers.Any())
                CompressedLayout.SetIsHeadless(layout, true);
        }

        public static void CancelAllAnimations(VisualElement element)
        {
            switch (element)
            {
                case ContentView contentView:
                    CancelAllAnimations(contentView?.Content);
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

        static IList<Layout<View>> GetChildLayouts(Layout<View> layout)
        {
            var childLayouts = layout?.Children?.OfType<Layout<View>>()?.ToList() ?? new List<Layout<View>>();

            var childContentViews = layout?.Children?.OfType<ContentView>()?.ToList() ?? new List<ContentView>();
            var childContentViewLayouts = childContentViews?.Where(x => x?.Content is Layout<View>)?.Select(x => x?.Content as Layout<View>)?.ToList() ?? new List<Layout<View>>();

            return childLayouts?.Concat(childContentViewLayouts)?.ToList() ?? new List<Layout<View>>();
        }
        #endregion
    }
}

