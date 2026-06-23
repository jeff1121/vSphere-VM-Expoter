using Backend.Models;

namespace Backend.Tests.Models;

public class LoginResponseTests
{
    [Fact]
    public void LoginResponse_DefaultValues_AreCorrect()
    {
        var response = new LoginResponse();

        Assert.False(response.Success);
        Assert.Equal(string.Empty, response.Message);
        Assert.Equal(string.Empty, response.SessionId);
    }

    [Fact]
    public void LoginResponse_CanSetProperties()
    {
        var response = new LoginResponse
        {
            Success = true,
            Message = "Login success",
            SessionId = "abc-123"
        };

        Assert.True(response.Success);
        Assert.Equal("Login success", response.Message);
        Assert.Equal("abc-123", response.SessionId);
    }
}
