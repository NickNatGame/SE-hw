using CPO.Interfaces;

namespace CPO.Domain;

public class ChargingStation : Thing, IEnergyConsumption
{
    public double DailyEnergyConsumption { get; }

    public ChargingStation(
        string name,
        string inventoryNumber,
        double dailyEnergyConsumption)
        : base(name, inventoryNumber)
    {
        if (!double.IsFinite(dailyEnergyConsumption) || dailyEnergyConsumption < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyEnergyConsumption), "Расход энергии должен быть конечным неотрицательным числом");
        }

        DailyEnergyConsumption = dailyEnergyConsumption;
    }
}
