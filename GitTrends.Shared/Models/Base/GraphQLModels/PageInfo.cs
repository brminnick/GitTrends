namespace GitTrends.Shared
{
    public record PageInfo(string EndCursor, bool HasNextPage, bool HasPreviousPage, string StartCursor);
}
