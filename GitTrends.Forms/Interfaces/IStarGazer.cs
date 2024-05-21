using System;

namespace GitTrends
{
	interface IStarGazerInfo
	{
		DateTimeOffset StarredAt { get; }
	}
}