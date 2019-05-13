using Xamarin.Forms;

namespace GitTrends
{
    public abstract class BaseContentPage<T> : ContentPage where T : BaseViewModel, new()
    {
        protected BaseContentPage() => BindingContext = ViewModel;

        protected T ViewModel { get; } = new T();
    }
}
