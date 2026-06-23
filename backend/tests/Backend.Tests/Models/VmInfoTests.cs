using Backend.Models;

namespace Backend.Tests.Models;

public class VmInfoTests
{
    [Fact]
    public void VmInfo_DefaultValues_AreCorrect()
    {
        var vm = new VmInfo();

        Assert.Equal(string.Empty, vm.Id);
        Assert.Equal(string.Empty, vm.Name);
        Assert.Equal(string.Empty, vm.PowerState);
        Assert.Null(vm.ProvisionedBytes);
    }

    [Fact]
    public void VmInfo_CanSetProperties()
    {
        var vm = new VmInfo
        {
            Id = "vm-100",
            Name = "My VM",
            PowerState = "POWERED_ON",
            ProvisionedBytes = 50_000_000_000L
        };

        Assert.Equal("vm-100", vm.Id);
        Assert.Equal("My VM", vm.Name);
        Assert.Equal("POWERED_ON", vm.PowerState);
        Assert.Equal(50_000_000_000L, vm.ProvisionedBytes);
    }
}
