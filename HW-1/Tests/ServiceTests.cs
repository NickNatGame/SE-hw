using CPO.Domain;
using CPO.Interfaces;
using CPO.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CPO.Tests;

public class ServiceTests
{
    private sealed class StubServiceCenter : IServiceCenter
    {
        private readonly bool _result;
        public List<Vehicle> InspectedVehicles { get; } = new List<Vehicle>();

        public StubServiceCenter(bool result)
        {
            _result = result;
        }

        public bool Inspect(Vehicle vehicle)
        {
            InspectedVehicles.Add(vehicle);
            return _result;
        }
    }

    [Fact]
    public void AddVehicle_AcceptsVehicleWhenDiStubApproves()
    {
        /* Это подставной сервис, который всегда выдает true*/
        var center = new StubServiceCenter(true);

        var registrations = new ServiceCollection();
        registrations.AddSingleton<IServiceCenter>(center);
        registrations.AddSingleton<Service>();
        using var provider = registrations.BuildServiceProvider();
        var service = provider.GetRequiredService<Service>();

        // Самокат неисправен, но подставной техосмотр разрешает приёмку
        var scooter = new Scooter("Самокат", "S1", 6, 0, false, 2.5);
        bool accepted = service.AddVehicle(scooter);

        Assert.True(accepted);
        Assert.Single(center.InspectedVehicles);
        Assert.Same(scooter, center.InspectedVehicles[0]);
        Assert.Single(service.GetInventory());
        Assert.Same(scooter, service.GetInventory()[0]);
        Assert.Single(service.GetVehiclesForBeginners());
        Assert.Same(scooter, service.GetVehiclesForBeginners()[0]);
        Assert.Equal(2.5, service.GetTotalEnergyConsumption());
    }

    [Fact]
    public void AddVehicle_RejectsVehicleWhenDiStubRejects()
    {
        /* То же самое наоборот */
        var center = new StubServiceCenter(false);
        var registrations = new ServiceCollection();
        registrations.AddSingleton<IServiceCenter>(center);
        registrations.AddSingleton<Service>();
        using var provider = registrations.BuildServiceProvider();
        var service = provider.GetRequiredService<Service>();

        // Самокат исправен, но подставной техосмотр запрещает приёмку
        var scooter = new Scooter("Самокат", "S1", 6, 100, true, 2.5);

        bool accepted = service.AddVehicle(scooter);

        Assert.False(accepted);
        Assert.Single(center.InspectedVehicles);
        Assert.Same(scooter, center.InspectedVehicles[0]);
        Assert.Empty(service.GetInventory());
        Assert.Empty(service.GetVehiclesForBeginners());
        Assert.Equal(0, service.GetTotalEnergyConsumption());
    }

    [Fact]
    public void EmptyPark_HasNoInventoryBeginnersOrEnergyConsumption()
    {
        var service = new Service(new ExpertiseServiceCenter());

        Assert.Empty(service.GetInventory());
        Assert.Empty(service.GetVehiclesForBeginners());
        Assert.Equal(0, service.GetTotalEnergyConsumption());
    }

    [Fact]
    public void Reports_IncludeOnlyAcceptedVehiclesAndAllThings()
    {
        var service = new Service(new ExpertiseServiceCenter());
        var scooter = new Scooter("Самокат", "S1", 6, 70, true, 2.5);
        var bike = new Bike("Велосипед", "B1", 5, 100, true);
        var wornScooter = new Scooter("Изношенный самокат", "S2", 10, 69, true, 100);
        var brokenBike = new Bike("Неисправный велосипед", "B2", 10, 100, false);
        var helmet = new Helmet("Шлем", "H1");
        var station = new ChargingStation("Станция", "C1", 10);

        Assert.True(service.AddVehicle(scooter));
        Assert.True(service.AddVehicle(bike));
        Assert.False(service.AddVehicle(wornScooter));
        Assert.False(service.AddVehicle(brokenBike));
        service.AddThing(helmet);
        service.AddThing(station);

        var inventory = service.GetInventory();
        Assert.Equal(4, inventory.Count);
        Assert.Same(scooter, inventory[0]);
        Assert.Same(bike, inventory[1]);
        Assert.Same(helmet, inventory[2]);
        Assert.Same(station, inventory[3]);
        Assert.Equal(12.5, service.GetTotalEnergyConsumption());
        Assert.Same(scooter, Assert.Single(service.GetVehiclesForBeginners()));
    }

    [Theory]
    [InlineData(5, false)]
    [InlineData(6, true)]
    public void GetVehiclesForBeginners_UsesEaseThreshold(int beginnerEase, bool expected)
    {
        var service = new Service(new ExpertiseServiceCenter());
        var bike = new Bike("Велосипед", "B1", beginnerEase, 80, true);
        Assert.True(service.AddVehicle(bike));

        Assert.Equal(expected, service.GetVehiclesForBeginners().Contains(bike));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void AddingItem_RejectsDuplicateIdAcrossVehiclesAndThings(bool existingIsVehicle, bool incomingIsVehicle)
    {
        var center = new StubServiceCenter(true);
        var service = new Service(center);

        if (existingIsVehicle)
        {
            Assert.True(service.AddVehicle(new Bike("Первый велосипед", "ID1", 6, 80, true)));
        }
        else
        {
            service.AddThing(new Helmet("Первый шлем", "ID1"));
        }

        if (incomingIsVehicle)
        {
            Assert.Throws<InvalidOperationException>(() =>
                service.AddVehicle(new Bike("Второй велосипед", " ID1 ", 6, 80, true)));
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => service.AddThing(new Helmet("Второй шлем", " ID1 ")));
        }

        Assert.Single(service.GetInventory());
        if (existingIsVehicle)
        {
            Assert.Single(center.InspectedVehicles);
        }
        else
        {
            Assert.Empty(center.InspectedVehicles);
        }
    }

    [Fact]
    public void RejectedVehicle_DoesNotReserveItsInventoryNumber()
    {
        var service = new Service(new ExpertiseServiceCenter());
        var rejected = new Scooter("Неисправный самокат", "ID1", 6, 69, true, 2.5);
        var replacement = new Bike("Исправный велосипед", "ID1", 6, 70, true);

        Assert.False(service.AddVehicle(rejected));
        Assert.True(service.AddVehicle(replacement));
        Assert.Same(replacement, Assert.Single(service.GetInventory()));
    }

    [Fact]
    public void Reports_ReturnSnapshotsWithoutExposingParkCollections()
    {
        var service = new Service(new ExpertiseServiceCenter());
        var first = new Bike("Первый велосипед", "B1", 6, 80, true);
        Assert.True(service.AddVehicle(first));
        var inventorySnapshot = service.GetInventory();
        var beginnerSnapshot = service.GetVehiclesForBeginners();

        Assert.True(service.AddVehicle(new Bike("Второй велосипед", "B2", 6, 80, true)));

        Assert.Same(first, Assert.Single(inventorySnapshot));
        Assert.Same(first, Assert.Single(beginnerSnapshot));

        inventorySnapshot.Clear();
        beginnerSnapshot.Clear();

        Assert.Equal(2, service.GetInventory().Count);
        Assert.Equal(2, service.GetVehiclesForBeginners().Count);
    }

    [Fact]
    public void Service_RejectsNullDependenciesAndItems()
    {
        Assert.Throws<ArgumentNullException>(() => new Service(null!));
        var service = new Service(new ExpertiseServiceCenter());

        Assert.Throws<ArgumentNullException>(() => service.AddVehicle(null!));
        Assert.Throws<ArgumentNullException>(() => service.AddThing(null!));
        Assert.Empty(service.GetInventory());
    }

}
