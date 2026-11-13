using CPO.Domain;
using CPO.Services;

namespace CPO.Tests;

public class ExpertiseServiceCenterTests
{
    [Theory]
    [InlineData(0, true, false)]
    [InlineData(69, true, false)]
    [InlineData(70, true, true)]
    [InlineData(100, true, true)]
    [InlineData(70, false, false)]
    [InlineData(100, false, false)]
    public void Inspect_RequiresConditionAtLeast70AndWorkingBrakes(int condition, bool brakesWork, bool expected)
    {
        var center = new ExpertiseServiceCenter();
        var bike = new Bike("Велосипед", "B1", 6, condition, brakesWork);
        var scooter = new Scooter("Самокат", "S1", 6, condition, brakesWork, 2.5);

        Assert.Equal(expected, center.Inspect(bike));
        Assert.Equal(expected, center.Inspect(scooter));
    }

    [Fact]
    public void Inspect_RejectsNullVehicle()
    {
        var center = new ExpertiseServiceCenter();

        Assert.Throws<ArgumentNullException>(() => center.Inspect(null!));
    }
}
