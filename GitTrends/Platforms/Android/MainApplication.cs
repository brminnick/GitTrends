using System.Net;
using Android.App;
using Android.Runtime;
using Xamarin.Android.Net;

namespace GitTrends;

#if AppStore
[Application(Debuggable = false)]
#else
[Application(Debuggable = true)]
#endif
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}