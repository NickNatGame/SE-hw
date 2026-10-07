public abstract class Vehicle : InventoryItem
{
    public string Name { get; }
    public string InventoryNumber { get; }
    public int Level { get; }
    
    public int Condition { get; }
    public bool BrakesWork { get; }

    protected Vehicle(string name, string inventoryNumber, int level, int condition, bool brakesWork)
    {
        Name = name;
        InventoryNumber = inventoryNumber;
        Level = level;
        Condition = condition;
        BrakesWork = brakesWork;
    }

    public bool IsForBeginners()
    {
        return Level >= 6;
    }

    public string GetInventoryNumber()
    {
        return InventoryNumber;
    }
}