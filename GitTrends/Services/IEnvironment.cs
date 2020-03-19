using System.Threading.Tasks;

namespace GitTrends
{
    public interface IEnvironment
    {
        Theme GetOperatingSystemTheme();
        Task<Theme> GetOperatingSystemThemeAsync();

        Task SetiOSBadgeCount(int count);
    }
}
