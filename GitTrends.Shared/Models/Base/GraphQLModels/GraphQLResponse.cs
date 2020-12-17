namespace GitTrends.Shared
{
    public record GraphQLResponse<T>(T Data, GraphQLError[] Errors);
}
