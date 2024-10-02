using Android.App;
using Android.Runtime;
using AndroidX.Emoji2.Text;

namespace GitTrends;

#if AppStore
[Application(Debuggable = false)]
#else
[Application(Debuggable = true)]
#endif
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
	{
		EmojiCompat.Init(Context);
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp(AppInfo.Current);
}