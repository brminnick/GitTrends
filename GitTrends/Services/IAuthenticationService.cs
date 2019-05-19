using System.Threading.Tasks;
namespace GitTrends
{
    public interface IAuthenticationService
    {
        Task LaunchWebAuthentication();
    }
}