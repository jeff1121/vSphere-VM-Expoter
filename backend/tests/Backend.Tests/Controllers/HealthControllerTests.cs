using Backend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Tests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsOkWithStatusOk()
    {
        var controller = new HealthController();

        var result = controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value;
        Assert.NotNull(value);
        var statusProp = value.GetType().GetProperty("status");
        Assert.NotNull(statusProp);
        Assert.Equal("ok", statusProp.GetValue(value));
    }
}
