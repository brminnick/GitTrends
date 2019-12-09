using System.Threading.Tasks;

namespace GitTrends
{
    public interface IEnvironment
    {
        Theme GetOperatingSystemTheme();
        ValueTask<Theme> GetOperatingSystemThemeAsync();
    }
}
