var scooter = new Scooter("Me", "oo1sdaw", 7, 2.5);
var service = new Service();
service.addVehicle(scooter);
if (service.GetInventory()[0] is EnergyConsumption item)
{
    Console.WriteLine(item.GetDailyEnergyConsumption());
}