using GitTrends.Common;
namespace GitTrends;

public interface IMobileReferringSiteModel : IReferringSiteModel
{
	string FavIconImageUrl { get; }
}