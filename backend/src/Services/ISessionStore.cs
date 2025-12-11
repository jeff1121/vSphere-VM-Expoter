namespace Backend.Services;

public record SessionData(string Host, string Token, string Username, string Password);

public interface ISessionStore
{
    void Add(string sessionId, SessionData data);
    SessionData? Get(string sessionId);
    void Remove(string sessionId);
}
