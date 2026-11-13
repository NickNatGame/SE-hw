using CPO.Domain;
using CPO.Interfaces;

namespace CPO.Services;

public class Service
{
    private readonly List<Vehicle> _vehicles;
    private readonly List<Thing> _things;
    private readonly IServiceCenter _serviceCenter;

    public Service(IServiceCenter serviceCenter)
    {
        if (serviceCenter == null)
        {
            throw new ArgumentNullException(nameof(serviceCenter));
        }

        _serviceCenter = serviceCenter;
        _vehicles = new List<Vehicle>();
        _things = new List<Thing>();
    }

    public bool AddVehicle(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            throw new ArgumentNullException(nameof(vehicle));
        }

        CheckInventoryNumber(vehicle.InventoryNumber);

        if (!_serviceCenter.Inspect(vehicle))
        {
            return false;
        }

        _vehicles.Add(vehicle);
        return true;
    }

    public void AddThing(Thing thing)
    {
        if (thing == null)
        {
            throw new ArgumentNullException(nameof(thing));
        }

        CheckInventoryNumber(thing.InventoryNumber);
        _things.Add(thing);
    }

    public List<Vehicle> GetVehiclesForBeginners()
    {
        List<Vehicle> result = new List<Vehicle>();

        foreach (Vehicle vehicle in _vehicles)
        {
            if (vehicle.IsForBeginners())
            {
                result.Add(vehicle);
            }
        }

        return result;
    }

    public List<IInventoryItem> GetInventory()
    {
        List<IInventoryItem> result = new List<IInventoryItem>();

        result.AddRange(_vehicles);
        result.AddRange(_things);

        return result;
    }

    public double GetTotalEnergyConsumption()
    {
        double totalEnergy = 0;
        List<IInventoryItem> inventory = GetInventory();

        foreach (IInventoryItem item in inventory)
        {
            if (item is IEnergyConsumption energyConsumer)
            {
                totalEnergy += energyConsumer.DailyEnergyConsumption;
            }
        }

        return totalEnergy;
    }

    private void CheckInventoryNumber(string inventoryNumber)
    {
        foreach (Vehicle vehicle in _vehicles)
        {
            if (vehicle.InventoryNumber == inventoryNumber)
            {
                throw new InvalidOperationException($"ID '{inventoryNumber}' уже используется");
            }
        }

        foreach (Thing thing in _things)
        {
            if (thing.InventoryNumber == inventoryNumber)
            {
                throw new InvalidOperationException($"ID '{inventoryNumber}' уже используется");
            }
        }
    }
}
