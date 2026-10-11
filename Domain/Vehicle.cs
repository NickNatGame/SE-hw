using CPO.Interfaces;

namespace CPO.Domain;

public abstract class Vehicle : IInventoryItem
{
    public string Name { get; }
    public string InventoryNumber { get; }
    public int BeginnerEase { get; }
    public int Condition { get; }
    public bool BrakesWork { get; }

    protected Vehicle(string name, string inventoryNumber, int beginnerEase, int condition, bool brakesWork)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название не должно быть пустым", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(inventoryNumber))
        {
            throw new ArgumentException("ID не должен быть пустым", nameof(inventoryNumber));
        }

        if (beginnerEase < 1 || beginnerEase > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(beginnerEase), "Уровень должен быть от 1 до 10");
        }

        if (condition < 0 || condition > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(condition), "Состояние должно быть от 0 до 100");
        }

        Name = name.Trim();
        InventoryNumber = inventoryNumber.Trim();
        BeginnerEase = beginnerEase;
        Condition = condition;
        BrakesWork = brakesWork;
    }

    public bool IsForBeginners()
    {
        return BeginnerEase >= 6;
    }
}