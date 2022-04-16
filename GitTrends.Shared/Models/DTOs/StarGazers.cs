using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
	public record StarGazers
	{
		public StarGazers(long totalCount, IEnumerable<StarGazerInfo> edges) =>
			(TotalCount, StarredAt) = (totalCount, edges.ToList());

		[JsonProperty("totalCount")]
		public long TotalCount { get; }

		[JsonProperty("edges")]
		public IReadOnlyList<StarGazerInfo> StarredAt { get; }
	}

	public record StarGazerInfo(DateTimeOffset StarredAt, string Cursor);

	public record StarGazer
	{
		public StarGazer(DateTimeOffset starred_at) => StarredAt = starred_at;

		public DateTimeOffset StarredAt { get; }
	}

	public record StarGazersConnection(long TotalCount);
}