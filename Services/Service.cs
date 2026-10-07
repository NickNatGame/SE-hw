public class Service
{
    private List<Vehicle> vehicles;
    private List<Thing> things;
    private ServiceCenter serviceCenter;

    public Service(ServiceCenter serviceCenter)
    {
        this.serviceCenter = serviceCenter;
        vehicles = new List<Vehicle>();
        things = new List<Thing>();
    }

    public bool addVehicle(Vehicle vehicle)
    {
        if (!serviceCenter.Inspect(vehicle))
        {
            return false;
        }
        vehicles.Add(vehicle);
        return true;
    }
    public void AddThing(Thing thing)
    {
        things.Add(thing);
    }

    public List<Vehicle> GetVehiclesForBeginners()
    {
        var result = new List<Vehicle>();

        foreach (Vehicle vehicle in vehicles)
        {
            if (vehicle.IsForBeginners())
            {
                result.Add(vehicle);
            }
        }

        return result;
    }

    public List<InventoryItem> GetInventory()
    {
        var result = new List<InventoryItem>();

        result.AddRange(vehicles);
        result.AddRange(things);

        return result;
    }

    double getTotalEnergyConsumption()
    {
        double totalEnergy = 0;
        foreach (Vehicle vehicle in vehicles)
        {
            if (vehicle is EnergyConsumption item)
            {
                totalEnergy += item.GetDailyEnergyConsumption();
            }
        }

        foreach (Thing thing in things)
        {
            if (thing is EnergyConsumption item)
            {
                totalEnergy += item.GetDailyEnergyConsumption();
            }
        }

        return totalEnergy;
    }
}