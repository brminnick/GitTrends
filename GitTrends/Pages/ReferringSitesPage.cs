using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class ReferringSitesPage : BaseContentPage<ReferringSitesViewModel>
    {
        readonly string _owner, _repository;

        public ReferringSitesPage(ReferringSitesViewModel refferringSitesViewModel, Repository repository) : base("Referring Sites", refferringSitesViewModel)
        {
            _owner = repository.OwnerLogin;
            _repository = repository.Name;

            var referringSitesLabel = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            };
            referringSitesLabel.SetBinding(Label.TextProperty, nameof(ReferringSitesViewModel.ReferringSitesLabelText));

            Content = referringSitesLabel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel.GetReferringSitesCommand.Execute((_owner, _repository));
        }
    }
}
