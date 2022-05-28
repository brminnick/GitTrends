using System;
using GitTrends.Shared;

namespace GitTrends.Mobile.Common
{
	public class AbuseRateLimitFound_GetReferringSitesEventArgs : EventArgs
	{
		public AbuseRateLimitFound_GetReferringSitesEventArgs(Repository repository) => Repository = repository;

		public Repository Repository { get; }
	}
}

