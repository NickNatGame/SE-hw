var scooter = new Scooter("Me", "oo1sdaw", 7, 80, true, 2.5);

var serviceCenter = new ExpertiseServiceCenter();
var service = new Service(serviceCenter);
service.addVehicle(scooter);
if (service.GetInventory()[0] is EnergyConsumption item)
{
    Console.WriteLine(item.GetDailyEnergyConsumption());
}