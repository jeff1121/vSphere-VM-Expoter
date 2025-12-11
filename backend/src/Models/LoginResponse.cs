namespace Backend.Models;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}
