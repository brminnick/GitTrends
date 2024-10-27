namespace GitTrends.Common;

public record StarGazerResponse(RepositoryStarGazers Repository);

public record RepositoryStarGazers(StarGazers StarGazers);