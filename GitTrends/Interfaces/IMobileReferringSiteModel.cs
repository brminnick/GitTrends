using GitTrends.Shared;
namespace GitTrends;

public interface IMobileReferringSiteModel : IReferringSiteModel
{
	string FavIconImageUrl { get; }
}