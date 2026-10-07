public class ChargingStation : Thing, EnergyConsumption
{
    public double DailyEnergyConsumption { get; }

    public ChargingStation(
        string name,
        string inventoryNumber,
        double dailyEnergyConsumption
    ) : base(name, inventoryNumber)
    {
        DailyEnergyConsumption = dailyEnergyConsumption;
    }

    public double GetDailyEnergyConsumption()
    {
        return DailyEnergyConsumption;
    }
}