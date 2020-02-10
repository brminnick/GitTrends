using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace GitTrends
{
    public class MobileReferringSiteModel : ReferringSiteModel
    {
        public MobileReferringSiteModel(ReferringSiteModel referringSiteModel, ImageSource? favIcon) : base(referringSiteModel.TotalCount, referringSiteModel.TotalUniqueCount, referringSiteModel.Referrer)
        {
            FavIcon = favIcon;
        }

        [JsonProperty("favIcon")]
        public ImageSource? FavIcon { get; }
    }
}
