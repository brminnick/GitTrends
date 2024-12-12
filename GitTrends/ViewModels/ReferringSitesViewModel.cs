using System.Net;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubApiStatus;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Refit;

namespace GitTrends;

public partial class ReferringSitesViewModel : BaseViewModel, IQueryAttributable
{
	public const string RepositoryQueryString = nameof(RepositoryQueryString);

	static readonly WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new();
	static readonly WeakEventManager<Repository> _abuseRateLimitFound_GetReferringSites_EventManager = new();

	readonly GitHubUserService _gitHubUserService;
	readonly ReferringSitesDatabase _referringSitesDatabase;
	readonly IGitHubApiStatusService _gitHubApiStatusService;
	readonly GitHubAuthenticationService _gitHubAuthenticationService;
	readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService;

    public ReferringSitesViewModel(IDispatcher mainThread,
									IAnalyticsService analyticsService,
									GitHubUserService gitHubUserService,
									ReferringSitesDatabase referringSitesDatabase,
									IGitHubApiStatusService gitHubApiStatusService,
									GitHubAuthenticationService gitHubAuthenticationService,
									GitHubApiRepositoriesService gitHubApiRepositoriesService) : base(analyticsService, mainThread)
	{
		_gitHubUserService = gitHubUserService;
		_referringSitesDatabase = referringSitesDatabase;
		_gitHubApiStatusService = gitHubApiStatusService;
		_gitHubAuthenticationService = gitHubAuthenticationService;
		_gitHubApiRepositoriesService = gitHubApiRepositoriesService;

		RetryGetReferringSitesJob.MobileReferringSiteRetrieved += HandleMobileReferringSiteRetrieved;

		RefreshState = RefreshState.Uninitialized;
	}
    
	[ObservableProperty]
	public partial bool IsRefreshing { get; set; }
    
	[ObservableProperty]
	public partial IReadOnlyList<MobileReferringSiteModel> MobileReferringSitesList { get; private set; } = [];

	[ObservableProperty]
	public partial string EmptyDataViewTitle { get; private set; } = string.Empty;

	[ObservableProperty]
	public partial string EmptyDataViewDescription { get; private set; } = string.Empty;

	[ObservableProperty]
	public partial bool IsEmptyDataViewEnabled { get; private set; }

	protected Repository? Repository { get; set; }

	public static event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
	{
		add => _pullToRefreshFailedEventManager.AddEventHandler(value);
		remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<Repository> AbuseRateLimitFound_GetReferringSites
	{
		add => _abuseRateLimitFound_GetReferringSites_EventManager.AddEventHandler(value);
		remove => _abuseRateLimitFound_GetReferringSites_EventManager.RemoveEventHandler(value);
	}

	RefreshState RefreshState
	{
		set
		{
			EmptyDataViewTitle = EmptyDataViewService.GetReferringSitesTitleText(value);
			EmptyDataViewDescription = EmptyDataViewService.GetReferringSitesDescriptionText(value);
		}
	}

	[RelayCommand(AllowConcurrentExecutions = true)]
	async Task ExecuteRefresh(CancellationToken cancellationToken)
	{
		if (Repository is null)
			return;

		var minimumTimerDisplayTimeTask = Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
		var referringSitesList = await GetReferringSites(Repository, cancellationToken).ConfigureAwait(false);
		MobileReferringSitesList = [.. MobileSortingService.SortReferringSites(referringSitesList.Select(static x => new MobileReferringSiteModel(x)))];

		if (!_gitHubUserService.IsDemoUser)
		{
			await foreach (var mobileReferringSite in _gitHubApiRepositoriesService.GetMobileReferringSites(referringSitesList, Repository.Url, cancellationToken).ConfigureAwait(false))
			{
				var referringSite = MobileReferringSitesList.Single(x => x.Referrer == mobileReferringSite.Referrer);
				referringSite.FavIcon = mobileReferringSite.FavIcon;

				if (!string.IsNullOrWhiteSpace(mobileReferringSite.FavIconImageUrl))
					await _referringSitesDatabase.SaveReferringSite(referringSite, Repository.Url, cancellationToken).ConfigureAwait(false);
			}
		}

		await minimumTimerDisplayTimeTask.ConfigureAwait(ConfigureAwaitOptions.None | ConfigureAwaitOptions.SuppressThrowing);

		IsRefreshing = false;
	}

	async Task<IReadOnlyList<ReferringSiteModel>> GetReferringSites(Repository repository, CancellationToken cancellationToken)
	{
		IReadOnlyList<ReferringSiteModel> referringSitesList = [];

		try
		{
			referringSitesList = await _gitHubApiRepositoriesService.GetReferringSites(repository, cancellationToken).ConfigureAwait(false);

			RefreshState = RefreshState.Succeeded;
		}
		catch (Exception e) when (e is ApiException { StatusCode: HttpStatusCode.Unauthorized })
		{
			OnPullToRefreshFailed(new LoginExpiredPullToRefreshEventArgs());

			await _gitHubAuthenticationService.LogOut(cancellationToken).ConfigureAwait(false);
		}
		catch (Exception e) when (_gitHubApiStatusService.IsAbuseRateLimit(e, out var retryDelay))
		{
			var mobileReferringSitesList = await _referringSitesDatabase.GetReferringSites(repository.Url, cancellationToken).ConfigureAwait(false);
			referringSitesList = [.. MobileSortingService.SortReferringSites(mobileReferringSitesList)];

			OnAbuseRateLimitFound_GetReferringSites(repository);
			OnPullToRefreshFailed(new AbuseLimitPullToRefreshEventArgs(retryDelay.Value, referringSitesList.Any()));
		}
		catch (Exception e) when (_gitHubApiStatusService.HasReachedMaximumApiCallLimit(e))
		{
			var responseHeaders = e switch
			{
				ApiException exception => exception.Headers,
				GraphQLException graphQLException => graphQLException.ResponseHeaders,
				_ => throw new NotSupportedException()
			};

			OnPullToRefreshFailed(new MaximumApiRequestsReachedEventArgs(_gitHubApiStatusService.GetRateLimitResetDateTime(responseHeaders)));
		}
		catch (Exception e)
		{
			OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs(ReferringSitesPageConstants.ErrorPullToRefreshEventArgs));

			AnalyticsService.Report(e);
		}
		finally
		{
			IsEmptyDataViewEnabled = true;
		}

		return referringSitesList;
	}

	void OnPullToRefreshFailed(in PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs)
	{
		RefreshState = pullToRefreshFailedEventArgs switch
		{
			ErrorPullToRefreshEventArgs => RefreshState.Error,
			AbuseLimitPullToRefreshEventArgs => RefreshState.AbuseLimit,
			LoginExpiredPullToRefreshEventArgs => RefreshState.LoginExpired,
			MaximumApiRequestsReachedEventArgs => RefreshState.MaximumApiLimit,
			_ => throw new NotSupportedException()
		};

		_pullToRefreshFailedEventManager.RaiseEvent(this, pullToRefreshFailedEventArgs, nameof(PullToRefreshFailed));
	}

	void OnAbuseRateLimitFound_GetReferringSites(in Repository repository) =>
		_abuseRateLimitFound_GetReferringSites_EventManager.RaiseEvent(this, repository, nameof(AbuseRateLimitFound_GetReferringSites));

	void HandleMobileReferringSiteRetrieved(object? sender, MobileReferringSiteModel e)
	{
		var updatedReferringSitesList = MobileReferringSitesList.Concat([e]);
		MobileReferringSitesList = [.. MobileSortingService.SortReferringSites(updatedReferringSitesList)];
	}

	void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
	{
		var repository = (Repository)query[RepositoryQueryString];
		Repository = repository;
	}
}