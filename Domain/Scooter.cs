using CPO.Interfaces;

namespace CPO.Domain;

public class Scooter : Vehicle, IEnergyConsumption
{
    public double DailyEnergyConsumption { get; }

    public Scooter(
        string name,
        string inventoryNumber,
        int beginnerEase,
        int condition,
        bool brakesWork,
        double dailyEnergyConsumption)
        : base(name, inventoryNumber, beginnerEase, condition, brakesWork)
    {
        if (!double.IsFinite(dailyEnergyConsumption) || dailyEnergyConsumption < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyEnergyConsumption), "Расход энергии должен быть конечным неотрицательным числом");
        }

        DailyEnergyConsumption = dailyEnergyConsumption;
    }
}
