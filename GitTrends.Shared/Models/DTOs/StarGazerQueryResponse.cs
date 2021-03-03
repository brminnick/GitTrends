namespace GitTrends.Shared
{
    public record StarGazerResponse(RepositoryStarGazers Repository);

    public record RepositoryStarGazers(StarGazers StarGazers);
}
