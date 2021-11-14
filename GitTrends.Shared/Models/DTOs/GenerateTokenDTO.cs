using System.ComponentModel;

namespace GitTrends.Shared
{
	public record GenerateTokenDTO(string LoginCode, string State);
}

// .NET 5 workaround https://stackoverflow.com/a/62656145/5953643
namespace System.Runtime.CompilerServices
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public record IsExternalInit { }
}