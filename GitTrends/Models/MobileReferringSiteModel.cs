using System;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace GitTrends
{
    public class MobileReferringSiteModel : ReferringSiteModel
    {
        const string DefaultFavIcon = "DefaultProfileImage";

        public MobileReferringSiteModel(ReferringSiteModel referringSiteModel, ImageSource? favIcon = null) : base(referringSiteModel.TotalCount, referringSiteModel.TotalUniqueCount, referringSiteModel.Referrer)
        {
            FavIcon = favIcon ?? DefaultFavIcon;
        }

        [JsonIgnore]
        public ImageSource FavIcon { get; }
    }
}
