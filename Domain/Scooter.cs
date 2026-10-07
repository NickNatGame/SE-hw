public class Scooter : Vehicle, EnergyConsumption
{
    public double DailyEnergyConsumption { get; }
    
    public Scooter(
        string name,
        string inventoryNumber,
        int level,
        int condition, 
        bool brakesWork,
        double dailyEnergyConsumption
    ) : base(name, inventoryNumber, level, condition, brakesWork)
    {
        DailyEnergyConsumption = dailyEnergyConsumption;
    }

    public double GetDailyEnergyConsumption()
    {
        return DailyEnergyConsumption;
    }
}