public class Scooter : Vehicle, EnergyConsumption
{
    public double DailyEnergyConsumption { get; }
    
    public Scooter(
        string name,
        string inventoryNumber,
        int level,
        double dailyEnergyConsumption
    ) : base(name, inventoryNumber, level)
    {
        DailyEnergyConsumption = dailyEnergyConsumption;
    }

    public double GetDailyEnergyConsumption()
    {
        return DailyEnergyConsumption;
    }
}