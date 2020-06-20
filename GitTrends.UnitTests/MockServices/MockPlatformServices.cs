using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.UnitTests;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using FileAccess = System.IO.FileAccess;
using FileMode = System.IO.FileMode;
using FileShare = System.IO.FileShare;
using Stream = System.IO.Stream;

[assembly: Dependency(typeof(MockDeserializer))]
[assembly: Dependency(typeof(MockResourcesProvider))]
namespace GitTrends.UnitTests
{
    class MockPlatformServices : IPlatformServices
    {
        readonly Action<Action>? _invokeOnMainThread;
        readonly Action<Uri>? _openUriAction;
        readonly Func<Uri, CancellationToken, Task<Stream>>? _getStreamAsync;
        readonly Func<VisualElement, double, double, SizeRequest>? _getNativeSizeFunc;
        readonly bool _useRealisticLabelMeasure;

        public MockPlatformServices(Action<Action>? invokeOnMainThread = null,
                                    Action<Uri>? openUriAction = null,
                                    Func<Uri, CancellationToken, Task<Stream>>? getStreamAsync = null,
                                    Func<VisualElement, double, double, SizeRequest>? getNativeSizeFunc = null,
                                    bool useRealisticLabelMeasure = false)
        {
            _invokeOnMainThread = invokeOnMainThread;
            _openUriAction = openUriAction;
            _getStreamAsync = getStreamAsync;
            _getNativeSizeFunc = getNativeSizeFunc;
            _useRealisticLabelMeasure = useRealisticLabelMeasure;
        }

        public OSAppTheme RequestedTheme { get; } = OSAppTheme.Unspecified;

        public bool IsInvokeRequired { get; } = false;

        public string RuntimePlatform { get; } = "Other";

        public string GetMD5Hash(string input) => throw new NotImplementedException();

        public string GetHash(string input) => throw new NotImplementedException();

        public double GetNamedSize(NamedSize size, Type targetElement, bool useOldSizes) => size switch
        {
            NamedSize.Default => 10,
            NamedSize.Micro => 4,
            NamedSize.Small => 8,
            NamedSize.Medium => 12,
            NamedSize.Large => 16,
            _ => throw new ArgumentOutOfRangeException(nameof(size)),
        };

        public Color GetNamedColor(string name) => name switch
        {
            "SystemBlue" => Color.FromRgb(0, 122, 255),
            "SystemChromeHighColor" => Color.FromHex("#FF767676"),
            "HoloBlueBright" => Color.FromHex("#ff00ddff"),
            _ => Color.Default,
        };

        public void OpenUriAction(Uri uri)
        {
            if (_openUriAction != null)
                _openUriAction(uri);
            else
                throw new NotImplementedException();
        }

        public void BeginInvokeOnMainThread(Action action)
        {
            if (_invokeOnMainThread is null)
                action();
            else
                _invokeOnMainThread(action);
        }

        public Ticker CreateTicker() => new MockTicker();

        public void StartTimer(TimeSpan interval, Func<bool> callback)
        {
            Timer? timer = null;
            timer = new Timer(onTimeout, null, interval, interval);

            void onTimeout(object? o) => BeginInvokeOnMainThread(() =>
            {
                if (callback())
                    return;

                timer?.Dispose();
            });
        }

        public Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (_getStreamAsync is null)
                throw new NotImplementedException();

            return _getStreamAsync(uri, cancellationToken);
        }

        public Assembly[] GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();

        public IIsolatedStorageFile GetUserStoreForApplication() => new MockIsolatedStorageFile(IsolatedStorageFile.GetUserStoreForAssembly());

        public class MockIsolatedStorageFile : IIsolatedStorageFile
        {
            readonly IsolatedStorageFile _isolatedStorageFile;

            public MockIsolatedStorageFile(IsolatedStorageFile isolatedStorageFile) =>
                _isolatedStorageFile = isolatedStorageFile;

            public Task<bool> GetDirectoryExistsAsync(string path) => Task.FromResult(_isolatedStorageFile.DirectoryExists(path));

            public Task CreateDirectoryAsync(string path)
            {
                _isolatedStorageFile.CreateDirectory(path);
                return Task.FromResult(true);
            }

            public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access)
            {
                Stream stream = _isolatedStorageFile.OpenFile(path, mode, access);
                return Task.FromResult(stream);
            }

            public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access, FileShare share)
            {
                Stream stream = _isolatedStorageFile.OpenFile(path, mode, access, share);
                return Task.FromResult(stream);
            }

            public Task<bool> GetFileExistsAsync(string path) => Task.FromResult(_isolatedStorageFile.FileExists(path));

            public Task<DateTimeOffset> GetLastWriteTimeAsync(string path) => Task.FromResult(_isolatedStorageFile.GetLastWriteTime(path));
        }

        public void QuitApplication()
        {

        }

        public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
        {
            if (_getNativeSizeFunc != null)
                return _getNativeSizeFunc(view, widthConstraint, heightConstraint);

            if (view is Label label && _useRealisticLabelMeasure)
            {
                var letterSize = new Size(5, 10);
                var w = label.Text.Length * letterSize.Width;
                var h = letterSize.Height;
                if (!double.IsPositiveInfinity(widthConstraint) && w > widthConstraint)
                {
                    h = ((int)w / (int)widthConstraint) * letterSize.Height;
                    w = widthConstraint - (widthConstraint % letterSize.Width);

                }
                return new SizeRequest(new Size(w, h), new Size(Math.Min(10, w), h));
            }

            return new SizeRequest(new Size(100, 20));
        }
    }

    class MockDeserializer : IDeserializer
    {
        public Task<IDictionary<string, object>> DeserializePropertiesAsync() =>
            Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>());

        public Task SerializePropertiesAsync(IDictionary<string, object> properties) => Task.FromResult(false);
    }

    class MockResourcesProvider : ISystemResourcesProvider
    {
        public IResourceDictionary GetSystemResources()
        {
            var dictionary = new ResourceDictionary();

            Style style;
            style = new Style(typeof(Label));
            dictionary[Device.Styles.BodyStyleKey] = style;

            style = new Style(typeof(Label));
            style.Setters.Add(Label.FontSizeProperty, 50);
            dictionary[Device.Styles.TitleStyleKey] = style;

            style = new Style(typeof(Label));
            style.Setters.Add(Label.FontSizeProperty, 40);
            dictionary[Device.Styles.SubtitleStyleKey] = style;

            style = new Style(typeof(Label));
            style.Setters.Add(Label.FontSizeProperty, 30);
            dictionary[Device.Styles.CaptionStyleKey] = style;

            style = new Style(typeof(Label));
            style.Setters.Add(Label.FontSizeProperty, 20);
            dictionary[Device.Styles.ListItemTextStyleKey] = style;

            style = new Style(typeof(Label));
            style.Setters.Add(Label.FontSizeProperty, 10);
            dictionary[Device.Styles.ListItemDetailTextStyleKey] = style;

            return dictionary;
        }
    }

    public class MockApplication : Application
    {
        public MockApplication()
        {
        }
    }

    class MockTicker : Ticker
    {
        bool _enabled;

        protected override void EnableTimer()
        {
            _enabled = true;

            while (_enabled)
            {
                SendSignals(16);
            }
        }

        protected override void DisableTimer() => _enabled = false;
    }
}