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
    void addThing(Thing thing)
    {
        things.Add(thing);
    }

    List<Vehicle> GetVehiclesForBeginners();
    List<InventoryItem> GetInventory();

    double getTotalEnergyConsumption();
}