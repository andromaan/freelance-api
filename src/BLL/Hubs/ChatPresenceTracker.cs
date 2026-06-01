using System.Collections.Concurrent;

namespace BLL.Hubs;

public class ChatPresenceTracker
{
    // Dictionary mapping UserId to a set of their active SignalR ConnectionIds
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _onlineUsers = new();

    public Task<bool> UserConnected(Guid userId, string connectionId)
    {
        bool isOnline = false;

        _onlineUsers.AddOrUpdate(
            userId,
            _ =>
            {
                isOnline = true; // This is the first connection, so they just came online
                return new HashSet<string> { connectionId };
            },
            (_, connections) =>
            {
                lock (connections)
                {
                    isOnline = connections.Count == 0; // If they had 0 connections somehow, they came online
                    connections.Add(connectionId);
                }
                return connections;
            });

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(Guid userId, string connectionId)
    {
        bool isOffline = false;

        if (_onlineUsers.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    isOffline = true;
                }
            }

            if (isOffline)
            {
                _onlineUsers.TryRemove(userId, out _);
            }
        }

        return Task.FromResult(isOffline);
    }

    public Task<bool> IsUserOnline(Guid userId)
    {
        return Task.FromResult(_onlineUsers.ContainsKey(userId));
    }
}
