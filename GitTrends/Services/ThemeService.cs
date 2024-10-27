using AsyncAwaitBestPractices;
using GitTrends.Common;
using GitTrends.Mobile.Common;

namespace GitTrends;

public class ThemeService(IAnalyticsService analyticsService, IPreferences preferences, IDispatcher dispatcher)
{
	static readonly WeakEventManager<PreferredTheme> _preferenceChangedEventManager = new();

	readonly IDispatcher _dispatcher = dispatcher;
	readonly IPreferences _preferences = preferences;
	readonly IAnalyticsService _analyticsService = analyticsService;

	public static event EventHandler<PreferredTheme> PreferenceChanged
	{
		add => _preferenceChangedEventManager.AddEventHandler(value);
		remove => _preferenceChangedEventManager.RemoveEventHandler(value);
	}

	public PreferredTheme Preference
	{
		get => (PreferredTheme)_preferences.Get(nameof(Preference), (int)PreferredTheme.Default);
		set
		{
			_preferences.Set(nameof(Preference), (int)value);
			SetAppTheme(value).SafeFireAndForget();
		}
	}

	public bool IsLightTheme() => Preference is PreferredTheme.Light || Preference is PreferredTheme.Default && Application.Current?.RequestedTheme is AppTheme.Light;
	public bool IsDarkTheme() => Preference is PreferredTheme.Dark || Preference is PreferredTheme.Default && Application.Current?.RequestedTheme is AppTheme.Dark;

	internal Task Initialize()
	{
		if (Application.Current is not null)
			Application.Current.RequestedThemeChanged += HandleRequestedThemeChanged;

		return SetAppTheme(Preference);
	}

	Task SetAppTheme(PreferredTheme preferredTheme)
	{
		if (Application.Current is null)
			return Task.CompletedTask;

		return _dispatcher.DispatchAsync(() =>
		{
			BaseTheme theme = preferredTheme switch
			{
				PreferredTheme.Dark => new DarkTheme(),
				PreferredTheme.Light => new LightTheme(),
				PreferredTheme.Default => Application.Current.RequestedTheme is AppTheme.Dark ? new DarkTheme() : new LightTheme(),
				_ => throw new NotSupportedException()
			};

			if (Application.Current.Resources.GetType() != theme.GetType())
			{
				Application.Current.Resources = theme;

				_analyticsService.Track("Theme Changed", new Dictionary<string, string>
				{
					{ nameof(PreferredTheme), preferredTheme.ToString() },
					{ nameof(Application.Current.RequestedTheme), Application.Current.RequestedTheme.ToString() }
				});

				OnPreferenceChanged(preferredTheme);
			}
		});
	}

	async void HandleRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
	{
		if (Preference is PreferredTheme.Default)
			await SetAppTheme(PreferredTheme.Default);
	}

	void OnPreferenceChanged(PreferredTheme theme) => _dispatcher.DispatchAsync(() => _preferenceChangedEventManager.RaiseEvent(this, theme, nameof(PreferenceChanged)));
}