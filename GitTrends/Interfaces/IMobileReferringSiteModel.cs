using GitTrends.Shared;
namespace GitTrends
{
    public interface IMobileReferringSiteModel : IReferringSiteModel
    {
        public string FavIconImageUrl { get; }
    }
}
