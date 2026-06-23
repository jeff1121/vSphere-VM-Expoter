using Backend.Models;
using Backend.Services;
using Backend.Services.InMemory;

namespace Backend.Tests.Services;

public class InMemorySessionStoreTests
{
    private readonly InMemorySessionStore _store = new();

    [Fact]
    public void Add_And_Get_ReturnsCorrectSession()
    {
        var sessionId = Guid.NewGuid().ToString();
        var data = new SessionData("host", "token", "user", "pass");

        _store.Add(sessionId, data);
        var result = _store.Get(sessionId);

        Assert.NotNull(result);
        Assert.Equal("host", result.Host);
        Assert.Equal("token", result.Token);
        Assert.Equal("user", result.Username);
        Assert.Equal("pass", result.Password);
    }

    [Fact]
    public void Get_ReturnsNull_WhenSessionNotFound()
    {
        var result = _store.Get("non-existent");

        Assert.Null(result);
    }

    [Fact]
    public void Remove_DeletesSession()
    {
        var sessionId = Guid.NewGuid().ToString();
        _store.Add(sessionId, new SessionData("host", "token", "user", "pass"));

        _store.Remove(sessionId);
        var result = _store.Get(sessionId);

        Assert.Null(result);
    }

    [Fact]
    public void Remove_DoesNotThrow_WhenSessionNotFound()
    {
        // Should not throw
        _store.Remove("non-existent");
    }

    [Fact]
    public void Add_Overwrites_ExistingSession()
    {
        var sessionId = Guid.NewGuid().ToString();
        _store.Add(sessionId, new SessionData("host1", "token1", "user1", "pass1"));
        _store.Add(sessionId, new SessionData("host2", "token2", "user2", "pass2"));

        var result = _store.Get(sessionId);

        Assert.NotNull(result);
        Assert.Equal("host2", result.Host);
        Assert.Equal("token2", result.Token);
    }
}
