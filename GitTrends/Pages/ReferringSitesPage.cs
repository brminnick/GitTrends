using System;
using System.Linq;
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
                Margin = new Thickness(10),
                HorizontalOptions = LayoutOptions.Start,
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

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            if (Navigation.ModalStack.Any())
                await Navigation.PopModalAsync();
        }
    }
}
