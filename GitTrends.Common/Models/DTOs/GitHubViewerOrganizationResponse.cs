namespace  GitTrends.Common;

public record GitHubViewerOrganizationResponse(ViewerOrganizationsResponse Viewer);

public record ViewerOrganizationsResponse(OrganizationNamesResponse Organizations);

public record OrganizationNamesResponse(IReadOnlyList<OrganizationRepositoryName> Nodes, PageInfo PageInfo);

public record OrganizationRepositoryName(string Login);