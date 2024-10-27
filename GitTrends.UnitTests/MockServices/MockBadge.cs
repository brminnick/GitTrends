using CommunityToolkit.Maui.ApplicationModel;
namespace GitTrends.UnitTests;

public class MockBadge : IBadge
{
	uint _count;

	public void SetCount(uint count)
	{
		_count = count;
	}
}