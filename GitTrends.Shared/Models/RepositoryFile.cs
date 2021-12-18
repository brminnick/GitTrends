using System;
namespace GitTrends.Shared;

public class RepositoryFile
{
	public RepositoryFile(string name, string sha, string path, Uri? download_url)
	{
		FileName = name;
		Sha = sha;
		Path = path;
		DownloadUrl = download_url;
	}

	public string FileName { get; }
	public string Sha { get; }
	public string Path { get; }
	public Uri? DownloadUrl { get; }
}