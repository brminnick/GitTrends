namespace GitTrends.Common;

public class RepositoryFile(string name, string sha, string path, Uri? download_url)
{
	public string FileName { get; } = name;
	public string Sha { get; } = sha;
	public string Path { get; } = path;
	public Uri? DownloadUrl { get; } = download_url;
}