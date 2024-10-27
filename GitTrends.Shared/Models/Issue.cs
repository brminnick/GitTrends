namespace  GitTrends.Common;

public record Issue(string Title, string Body, DateTimeOffset CreatedAt, string State, DateTimeOffset? ClosedAt = null);