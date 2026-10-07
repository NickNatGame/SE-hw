public abstract class Vehicle : InventoryItem
{
    public string Name { get; }
    public string InventoryNumber { get; }
    public int Level { get; }

    protected Vehicle(string name, string inventoryNumber, int level)
    {
        Name = name;
        InventoryNumber = inventoryNumber;
        Level = level;
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