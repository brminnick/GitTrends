using Shiny;
using Shiny.Notifications;

namespace GitTrends.UnitTests;

public class MockNotificationManager : INotificationManager
{
	readonly Dictionary<string, Channel> _channelsDictionary = [];
	readonly Dictionary<int, Notification> _pendingNotificationsDictionary = [];

	public Task Cancel(int id)
	{
		_pendingNotificationsDictionary.Remove(id);
		return Task.CompletedTask;
	}

	public Task Cancel(CancelScope cancelScope = CancelScope.All)
	{
		switch (cancelScope)
		{
			case CancelScope.Pending:
			case CancelScope.All:
				_pendingNotificationsDictionary.Clear();
				break;

			case CancelScope.DisplayedOnly:
				break;

			default:
				throw new System.NotSupportedException($"{cancelScope} not supported");
		}

		return Task.CompletedTask;
	}

	public Task<IList<Notification>> GetPendingNotifications() => Task.FromResult<IList<Notification>>([.. _pendingNotificationsDictionary.Values]);

	public Task<AccessState> RequestAccess(AccessRequestFlags flags = AccessRequestFlags.Notification)
	{
		throw new NotImplementedException();
		return Task.FromResult(AccessState.Available);
	}

	public Task Send(Notification notification)
	{
		_pendingNotificationsDictionary.Add(notification.Id, notification);
		return Task.CompletedTask;
	}

	public void AddChannel(Channel channel)
	{
		_channelsDictionary.Add(channel.Identifier, channel);
	}

	public void ClearChannels()
	{
		_channelsDictionary.Clear();
	}

	public IList<Channel> GetChannels() => [.. _channelsDictionary.Values];

	public Channel? GetChannel(string channelId) => _channelsDictionary.GetValueOrDefault(channelId);

	public void RemoveChannel(string channelId)
	{
		_channelsDictionary.Remove(channelId);
	}

	public Task<Notification> GetNotification(int notificationId) => Task.FromResult<Notification>(_pendingNotificationsDictionary[notificationId]);
}