using CPO.Domain;

namespace CPO.Tests;

public class DomainTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void Constructors_RejectEmptyNames(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Bike(name!, "B1", 6, 80, true));
        Assert.ThrowsAny<ArgumentException>(() => new Helmet(name!, "H1"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void Constructors_RejectEmptyInventoryNumbers(string? inventoryNumber)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Bike("Велосипед", inventoryNumber!, 6, 80, true));
        Assert.ThrowsAny<ArgumentException>(() => new Helmet("Шлем", inventoryNumber!));
    }

    [Fact]
    public void Constructors_TrimNamesAndInventoryNumbers()
    {
        var bike = new Bike(" Велосипед ", " B1 ", 6, 80, true);
        var helmet = new Helmet(" Шлем ", " H1 ");

        Assert.Equal("Велосипед", bike.Name);
        Assert.Equal("B1", bike.InventoryNumber);
        Assert.Equal("Шлем", helmet.Name);
        Assert.Equal("H1", helmet.InventoryNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Vehicles_RejectBeginnerEaseOutsideRange(int beginnerEase)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Bike("Велосипед", "B1", beginnerEase, 80, true));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Scooter("Самокат", "S1", beginnerEase, 80, true, 2.5));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Vehicles_RejectConditionOutsideRange(int condition)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Bike("Велосипед", "B1", 6, condition, true));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Scooter("Самокат", "S1", 6, condition, true, 2.5));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(10, 100)]
    public void Vehicles_AcceptRangeEndpoints(int beginnerEase, int condition)
    {
        var bike = new Bike("Велосипед", "B1", beginnerEase, condition, true);
        var scooter = new Scooter("Самокат", "S1", beginnerEase, condition, true, 0);

        Assert.Equal(beginnerEase, bike.BeginnerEase);
        Assert.Equal(condition, bike.Condition);
        Assert.Equal(beginnerEase, scooter.BeginnerEase);
        Assert.Equal(condition, scooter.Condition);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(5, false)]
    [InlineData(6, true)]
    [InlineData(10, true)]
    public void IsForBeginners_UsesEaseThreshold(int beginnerEase, bool expected)
    {
        var bike = new Bike("Велосипед", "B1", beginnerEase, 80, true);
        var scooter = new Scooter("Самокат", "S1", beginnerEase, 80, true, 2.5);

        Assert.Equal(expected, bike.IsForBeginners());
        Assert.Equal(expected, scooter.IsForBeginners());
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void EnergyConsumers_RejectNegativeOrNonFiniteEnergy(double energy)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Scooter("Самокат", "S1", 6, 80, true, energy));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChargingStation("Станция", "C1", energy));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2.5)]
    public void EnergyConsumers_AcceptFiniteNonNegativeEnergy(double energy)
    {
        var scooter = new Scooter("Самокат", "S1", 6, 80, true, energy);
        var station = new ChargingStation("Станция", "C1", energy);

        Assert.Equal(energy, scooter.DailyEnergyConsumption);
        Assert.Equal(energy, station.DailyEnergyConsumption);
    }
}
