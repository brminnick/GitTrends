using Microsoft.Extensions.DependencyInjection;
using Shiny;

namespace GitTrends
{
    public class ShinyStartup : Shiny.ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services, IPlatform platform) => services.UseNotifications();
    }
}
