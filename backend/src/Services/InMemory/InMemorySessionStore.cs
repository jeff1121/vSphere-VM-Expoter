using System.Collections.Concurrent;

namespace Backend.Services.InMemory;

public class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, SessionData> _sessions = new();

    public void Add(string sessionId, SessionData data)
    {
        _sessions[sessionId] = data;
    }

    public SessionData? Get(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var data);
        return data;
    }

    public void Remove(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }
}
