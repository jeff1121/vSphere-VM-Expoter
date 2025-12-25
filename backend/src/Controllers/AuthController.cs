using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IVSphereService _vsphereService;

    public AuthController(IVSphereService vsphereService)
    {
        _vsphereService = vsphereService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Host) || string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new LoginResponse { Success = false, Message = "Host 與 Username 為必填" });
        }

        try
        {
            var sessionId = await _vsphereService.LoginAsync(request, cancellationToken);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login success",
                SessionId = sessionId,
            });
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return Unauthorized(new LoginResponse { Success = false, Message = "登入失敗：帳號或密碼錯誤" });
            }
            return StatusCode(500, new LoginResponse { Success = false, Message = $"連線錯誤: {ex.Message}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new LoginResponse { Success = false, Message = $"系統錯誤: {ex.Message}" });
        }
    }
}
