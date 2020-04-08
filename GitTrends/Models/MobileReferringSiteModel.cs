using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace GitTrends
{
    public class MobileReferringSiteModel : ReferringSiteModel
    {
        public MobileReferringSiteModel(in ReferringSiteModel referringSiteModel, in ImageSource? favIcon = null)
            : base(referringSiteModel.TotalCount, referringSiteModel.TotalUniqueCount, referringSiteModel.Referrer)
        {
            FavIcon = favIcon;
        }

        [JsonIgnore]
        public ImageSource? FavIcon { get; }
    }
}
