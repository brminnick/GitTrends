using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;

namespace GitTrends
{
    public class ReferringSitesViewModel : BaseViewModel
    {
        readonly GitHubApiV3Service _gitHubApiV3Service;

        string _referringSitesLabelText = string.Empty;

        public ReferringSitesViewModel(GitHubApiV3Service gitHubApiV3Service)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
            GetReferringSitesCommand = new AsyncCommand<(string Owner, string Repository)>(repo => ExecuteGetReferringSitesCommand(repo.Owner, repo.Repository));
        }

        public string ReferringSitesLabelText
        {
            get => _referringSitesLabelText;
            set => SetProperty(ref _referringSitesLabelText, value);
        }

        public ICommand GetReferringSitesCommand { get; }

        async Task ExecuteGetReferringSitesCommand(string owner, string repository)
        {
            ReferringSitesLabelText = string.Empty;

            var referringSites = await _gitHubApiV3Service.GetReferingSites(owner, repository).ConfigureAwait(false);

            foreach (var site in referringSites)
                ReferringSitesLabelText += $"{nameof(ReferingSiteModel.Referrer)}: {site.Referrer}\n{nameof(ReferingSiteModel.TotalCount)}: {site.TotalCount}\n{nameof(ReferingSiteModel.TotalUniqueCount)}: {site.TotalUniqueCount}";
         }
    }
}
