using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitHubApiStatus;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class ReferringSitesViewModel : BaseViewModel
    {
        readonly static WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new();
        readonly static WeakEventManager<Repository> _abuseRateLimitFound_GetReferringSites_EventManager = new();

        readonly FavIconService _favIconService;
        readonly GitHubUserService _gitHubUserService;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly ReferringSitesDatabase _referringSitesDatabase;
        readonly IGitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService;

        IReadOnlyList<MobileReferringSiteModel>? _mobileReferringSiteList;

        string _emptyDataViewText = string.Empty;
        string _emptyDataViewDescription = string.Empty;

        bool _isRefreshing, _isEmptyDataViewEnabled;

        public ReferringSitesViewModel(IMainThread mainThread,
                                        FavIconService favIconService,
                                        IAnalyticsService analyticsService,
                                        GitHubUserService gitHubUserService,
                                        GitHubApiV3Service gitHubApiV3Service,
                                        ReferringSitesDatabase referringSitesDatabase,
                                        GitHubApiStatusService gitHubApiStatusService,
                                        GitHubAuthenticationService gitHubAuthenticationService,
                                        GitHubApiRepositoriesService gitHubApiRepositoriesService) : base(analyticsService, mainThread)
        {
            _favIconService = favIconService;
            _gitHubUserService = gitHubUserService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _referringSitesDatabase = referringSitesDatabase;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _gitHubApiRepositoriesService = gitHubApiRepositoriesService;

            RefreshState = RefreshState.Uninitialized;

            RefreshCommand = new AsyncCommand<(Repository Repository, CancellationToken Token)>(tuple => ExecuteRefreshCommand(tuple.Repository, tuple.Token));
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

        public IAsyncCommand<(Repository repository, CancellationToken Token)> RefreshCommand { get; }

        public string EmptyDataViewTitle
        {
            get => _emptyDataViewText;
            set => SetProperty(ref _emptyDataViewText, value);
        }

        public string EmptyDataViewDescription
        {
            get => _emptyDataViewDescription;
            set => SetProperty(ref _emptyDataViewDescription, value);
        }

        public bool IsEmptyDataViewEnabled
        {
            get => _isEmptyDataViewEnabled;
            set => SetProperty(ref _isEmptyDataViewEnabled, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public IReadOnlyList<MobileReferringSiteModel> MobileReferringSitesList
        {
            get => _mobileReferringSiteList ??= Array.Empty<MobileReferringSiteModel>();
            set => SetProperty(ref _mobileReferringSiteList, value);
        }

        RefreshState RefreshState
        {
            set
            {
                EmptyDataViewTitle = EmptyDataViewService.GetReferringSitesTitleText(value);
                EmptyDataViewDescription = EmptyDataViewService.GetReferringSitesDescriptionText(value);
            }
        }

        async Task ExecuteRefreshCommand(Repository repository, CancellationToken cancellationToken)
        {
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
    }
}
