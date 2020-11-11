namespace GitTrends.Shared
{
    public record GenerateTokenDTO(string LoginCode, string State);
}

#warning .NET 5 workaround https://stackoverflow.com/a/62656145/5953643
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}