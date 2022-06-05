using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubApiStatus;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public partial class ReferringSitesViewModel : BaseViewModel
	{
		readonly static WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new();
		readonly static WeakEventManager<Repository> _abuseRateLimitFound_GetReferringSites_EventManager = new();

		readonly GitHubUserService _gitHubUserService;
		readonly ReferringSitesDatabase _referringSitesDatabase;
		readonly IGitHubApiStatusService _gitHubApiStatusService;
		readonly GitHubAuthenticationService _gitHubAuthenticationService;
		readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService;

		[ObservableProperty]
		IReadOnlyList<MobileReferringSiteModel> _mobileReferringSitesList = Array.Empty<MobileReferringSiteModel>();

		[ObservableProperty]
		string _emptyDataViewTitle = string.Empty, _emptyDataViewDescription = string.Empty;

		[ObservableProperty]
		bool _isRefreshing, _isEmptyDataViewEnabled;

		public ReferringSitesViewModel(IMainThread mainThread,
										IAnalyticsService analyticsService,
										GitHubUserService gitHubUserService,
										ReferringSitesDatabase referringSitesDatabase,
										GitHubApiStatusService gitHubApiStatusService,
										GitHubAuthenticationService gitHubAuthenticationService,
										GitHubApiRepositoriesService gitHubApiRepositoriesService) : base(analyticsService, mainThread)
		{
			_gitHubUserService = gitHubUserService;
			_referringSitesDatabase = referringSitesDatabase;
			_gitHubApiStatusService = gitHubApiStatusService;
			_gitHubAuthenticationService = gitHubAuthenticationService;
			_gitHubApiRepositoriesService = gitHubApiRepositoriesService;

			BackgroundFetchService.MobileReferringSiteRetrieved += HandleMobileReferringSiteRetrieved;

			RefreshState = RefreshState.Uninitialized;
		}

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

		[ICommand(AllowConcurrentExecutions = true)]
		async Task ExecuteRefresh((Repository repository, CancellationToken cancellationToken) parameter)
		{
			var (repository, cancellationToken) = parameter;

			var referringSitesList = await GetReferringSites(repository, cancellationToken).ConfigureAwait(false);
			MobileReferringSitesList = MobileSortingService.SortReferringSites(referringSitesList.Select(x => new MobileReferringSiteModel(x))).ToList();

			if (!_gitHubUserService.IsDemoUser)
			{
				await foreach (var mobileReferringSite in _gitHubApiRepositoriesService.GetMobileReferringSites(referringSitesList, repository.Url, cancellationToken).ConfigureAwait(false))
				{
					var referringSite = MobileReferringSitesList.Single(x => x.Referrer == mobileReferringSite.Referrer);
					referringSite.FavIcon = mobileReferringSite.FavIcon;

					if (string.IsNullOrWhiteSpace(mobileReferringSite.FavIconImageUrl))
						await _referringSitesDatabase.SaveReferringSite(referringSite, repository.Url).ConfigureAwait(false);
				}
			}

			IsRefreshing = false;
		}

		async Task<IReadOnlyList<ReferringSiteModel>> GetReferringSites(Repository repository, CancellationToken cancellationToken)
		{
			IReadOnlyList<ReferringSiteModel> referringSitesList = Array.Empty<ReferringSiteModel>();

			try
			{
				referringSitesList = await _gitHubApiRepositoriesService.GetReferringSites(repository, cancellationToken).ConfigureAwait(false);

				RefreshState = RefreshState.Succeeded;
			}
			catch (Exception e) when (e is ApiException { StatusCode: HttpStatusCode.Unauthorized })
			{
				OnPullToRefreshFailed(new LoginExpiredPullToRefreshEventArgs());

				await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);
			}
			catch (Exception e) when (_gitHubApiStatusService.IsAbuseRateLimit(e, out var retryDelay))
			{
				var mobileReferringSitesList = await _referringSitesDatabase.GetReferringSites(repository.Url).ConfigureAwait(false);
				referringSitesList = MobileSortingService.SortReferringSites(mobileReferringSitesList).ToList();

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

		void HandleMobileReferringSiteRetrieved(object sender, MobileReferringSiteModel e)
		{
			var updatedReferringSitesList = MobileReferringSitesList.Concat(new List<MobileReferringSiteModel> { e });
			MobileReferringSitesList = MobileSortingService.SortReferringSites(updatedReferringSitesList).ToList();
		}
	}
}