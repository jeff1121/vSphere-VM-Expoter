using Backend.Models;

namespace Backend.Tests.Models;

public class LoginRequestTests
{
    [Fact]
    public void LoginRequest_DefaultValues_AreCorrect()
    {
        var request = new LoginRequest();

        Assert.Equal(string.Empty, request.Host);
        Assert.Equal(string.Empty, request.Username);
        Assert.Equal(string.Empty, request.Password);
    }

    [Fact]
    public void LoginRequest_CanSetProperties()
    {
        var request = new LoginRequest
        {
            Host = "vcenter.local",
            Username = "admin",
            Password = "secret123"
        };

        Assert.Equal("vcenter.local", request.Host);
        Assert.Equal("admin", request.Username);
        Assert.Equal("secret123", request.Password);
    }
}
