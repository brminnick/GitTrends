using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
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

        readonly FavIconService _favIconService;
        readonly GitHubUserService _gitHubUserService;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly ReferringSitesDatabase _referringSitesDatabase;
        readonly GitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubAuthenticationService _gitHubAuthenticationService;

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
                                        GitHubAuthenticationService gitHubAuthenticationService) : base(analyticsService, mainThread)
        {
            _favIconService = favIconService;
            _gitHubUserService = gitHubUserService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _referringSitesDatabase = referringSitesDatabase;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubAuthenticationService = gitHubAuthenticationService;

            RefreshState = RefreshState.Uninitialized;

            RefreshCommand = new AsyncCommand<(string Owner, string Repository, string RepositoryUrl, CancellationToken Token)>(tuple => ExecuteRefreshCommand(tuple.Owner, tuple.Repository, tuple.RepositoryUrl, tuple.Token));
        }

        public static event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public IAsyncCommand<(string Owner, string Repository, string RepositoryUrl, CancellationToken Token)> RefreshCommand { get; }

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

        async Task ExecuteRefreshCommand(string owner, string repository, string repositoryUrl, CancellationToken cancellationToken)
        {
            var referringSitesList = await GetReferringSites(owner, repository, cancellationToken).ConfigureAwait(false);

            try
            {
                if (!_gitHubUserService.IsDemoUser)
                {
                    await foreach (var mobileReferringSite in GetMobileReferringSiteWithFavIconList(referringSitesList, repositoryUrl, cancellationToken).ConfigureAwait(false))
                    {
                        var referringSite = MobileReferringSitesList.Single(x => x.Referrer == mobileReferringSite.Referrer);
                        referringSite.FavIcon = mobileReferringSite.FavIcon;
                    }

                    foreach (var referringSite in MobileReferringSitesList)
                        await _referringSitesDatabase.SaveReferringSite(referringSite, repositoryUrl).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                //Let's track the exception, but we don't need to do anything with it because the data still appears, just without the icons
                AnalyticsService.Report(e);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        async Task<IReadOnlyList<ReferringSiteModel>> GetReferringSites(string owner, string repository, CancellationToken cancellationToken)
        {
            HttpResponseMessage? finalResponse = null;
            IReadOnlyList<ReferringSiteModel> referringSitesList = Array.Empty<ReferringSiteModel>();

            try
            {
                referringSitesList = await _gitHubApiV3Service.GetReferringSites(owner, repository, cancellationToken).ConfigureAwait(false);

                MobileReferringSitesList = MobileSortingService.SortReferringSites(referringSitesList.Select(x => new MobileReferringSiteModel(x))).ToList();

                if (!_gitHubUserService.IsDemoUser)
                {
                    //Call EnsureSuccessStatusCode to confirm the above API calls executed successfully
                    finalResponse = await _gitHubApiV3Service.GetGitHubApiResponse(cancellationToken).ConfigureAwait(false);
                    finalResponse.EnsureSuccessStatusCode();
                }

                RefreshState = RefreshState.Succeeded;
            }
            catch (Exception e) when ((e is ApiException exception && exception.StatusCode is HttpStatusCode.Unauthorized)
                                        || (e is HttpRequestException && finalResponse != null && finalResponse.StatusCode is HttpStatusCode.Unauthorized))
            {
                OnPullToRefreshFailed(new LoginExpiredPullToRefreshEventArgs());

                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);

                RefreshState = RefreshState.LoginExpired;
            }
            catch (Exception e) when (_gitHubApiStatusService.HasReachedMaximumApiCallLimit(e)
                                        || (e is HttpRequestException && finalResponse != null && _gitHubApiStatusService.HasReachedMaximimApiCallLimit(finalResponse.Headers)))
            {
                var responseHeaders = e switch
                {
                    ApiException exception => exception.Headers,
                    GraphQLException graphQLException => graphQLException.ResponseHeaders,
                    HttpRequestException when finalResponse != null => finalResponse.Headers,
                    _ => throw new NotSupportedException()
                };

                OnPullToRefreshFailed(new MaximumApiRequestsReachedEventArgs(_gitHubApiStatusService.GetRateLimitResetDateTime(responseHeaders)));

                RefreshState = RefreshState.MaximumApiLimit;
            }
            catch (Exception e)
            {
                OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs(ReferringSitesPageConstants.ErrorPullToRefreshEventArgs));

                AnalyticsService.Report(e);

                RefreshState = RefreshState.Error;
            }
            finally
            {
                IsEmptyDataViewEnabled = true;
            }

            return referringSitesList;
        }


        async IAsyncEnumerable<MobileReferringSiteModel> GetMobileReferringSiteWithFavIconList(IEnumerable<ReferringSiteModel> referringSites, string repositoryUrl, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var favIconTaskList = referringSites.Select(x => setFavIcon(_referringSitesDatabase, x, repositoryUrl, cancellationToken)).ToList();

            while (favIconTaskList.Any())
            {
                var completedFavIconTask = await Task.WhenAny(favIconTaskList).ConfigureAwait(false);
                favIconTaskList.Remove(completedFavIconTask);

                var mobileReferringSiteModel = await completedFavIconTask.ConfigureAwait(false);
                yield return mobileReferringSiteModel;
            }

            async Task<MobileReferringSiteModel> setFavIcon(ReferringSitesDatabase referringSitesDatabase, ReferringSiteModel referringSiteModel, string repositoryUrl, CancellationToken cancellationToken)
            {
                var mobileReferringSiteFromDatabase = await referringSitesDatabase.GetReferringSite(repositoryUrl, referringSiteModel.ReferrerUri).ConfigureAwait(false);

                if (mobileReferringSiteFromDatabase != null && isFavIconValid(mobileReferringSiteFromDatabase))
                    return mobileReferringSiteFromDatabase;

                if (_gitHubUserService.IsDemoUser)
                {
                    //Display the Activity Indicator to ensure consistent UX
                    await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                    return new MobileReferringSiteModel(referringSiteModel, FavIconService.DefaultFavIcon);
                }
                else if (referringSiteModel.ReferrerUri != null && referringSiteModel.IsReferrerUriValid)
                {
                    var favIcon = await _favIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri, cancellationToken).ConfigureAwait(false);
                    return new MobileReferringSiteModel(referringSiteModel, favIcon);
                }
                else
                {
                    return new MobileReferringSiteModel(referringSiteModel, FavIconService.DefaultFavIcon);
                }

                static bool isFavIconValid(MobileReferringSiteModel mobileReferringSiteModel) => !string.IsNullOrWhiteSpace(mobileReferringSiteModel.FavIconImageUrl) && mobileReferringSiteModel.DownloadedAt.CompareTo(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(30))) > 0;
            }
        }

        void OnPullToRefreshFailed(PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs) =>
            _pullToRefreshFailedEventManager.RaiseEvent(this, pullToRefreshFailedEventArgs, nameof(PullToRefreshFailed));
    }
}
