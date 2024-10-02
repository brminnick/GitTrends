using Microsoft.Extensions.DependencyInjection;
using Shiny;

namespace GitTrends
{
	public class ShinyStartup : Shiny.ShinyStartup
	{
		readonly IDeviceNotificationsService _deviceNotificationsService;

		public ShinyStartup(IDeviceNotificationsService deviceNotificationsService) =>
			_deviceNotificationsService = deviceNotificationsService;

		public override void ConfigureServices(IServiceCollection services, IPlatform platform)
		{
			services.AddSingleton<IDeviceNotificationsService>(_deviceNotificationsService);

			services.UseJobs();
			services.UseNotifications();
		}
	}
}