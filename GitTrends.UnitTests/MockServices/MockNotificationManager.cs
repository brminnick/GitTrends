using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shiny;
using Shiny.Notifications;

namespace GitTrends.UnitTests
{
	public class MockNotificationManager : INotificationManager
	{
		readonly Dictionary<string, Channel> _channelsDictionary = new();
		readonly Dictionary<int, Notification> _pendingNotificationsDitcionary = new();
		readonly IDeviceNotificationsService _deviceNotificationsService;

		int _badge;

		public MockNotificationManager(IDeviceNotificationsService deviceNotificationsService) =>
			_deviceNotificationsService = deviceNotificationsService;

		public Task<int> GetBadge() => Task.FromResult(_badge);

		public Task SetBadge(int? badge)
		{
			badge ??= 0;

			_badge = badge.Value;

			return Task.CompletedTask;
		}

		public Task Cancel(int id)
		{
			_pendingNotificationsDitcionary.Remove(id);
			return Task.CompletedTask;
		}

		public Task Cancel(CancelScope cancelScope = CancelScope.All)
		{
			switch (cancelScope)
			{
				case CancelScope.Pending:
				case CancelScope.All:
					_pendingNotificationsDitcionary.Clear();
					break;

				case CancelScope.DisplayedOnly:
					break;

				default:
					throw new System.NotSupportedException($"{cancelScope} not supported");
			}

			return Task.CompletedTask;
		}

		public Task<IEnumerable<Notification>> GetPendingNotifications() => Task.FromResult(_pendingNotificationsDitcionary.Values.AsEnumerable());

		public Task<AccessState> RequestAccess(AccessRequestFlags flags = AccessRequestFlags.Notification)
		{
			_deviceNotificationsService.Initialize();
			return Task.FromResult(AccessState.Available);
		}

		public Task Send(Notification notification)
		{
			_pendingNotificationsDitcionary.Add(notification.Id, notification);
			return Task.CompletedTask;
		}

		public Task AddChannel(Channel channel)
		{
			_channelsDictionary.Add(channel.Identifier, channel);
			return Task.CompletedTask;
		}

		public Task ClearChannels()
		{
			_channelsDictionary.Clear();
			return Task.CompletedTask;
		}

		public Task<IList<Channel>> GetChannels() => Task.FromResult((IList<Channel>)_channelsDictionary.Values.ToList());

		public Task RemoveChannel(string channelId)
		{
			_channelsDictionary.Remove(channelId);
			return Task.CompletedTask;
		}

		public Task<Notification?> GetNotification(int notificationId) => Task.FromResult<Notification?>(_pendingNotificationsDitcionary[notificationId]);
	}
}