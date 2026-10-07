public abstract class Thing : InventoryItem
{
    public string Name { get; }
    public string InventoryNumber { get; }

    protected Thing(string name, string inventoryNumber)
    {
        Name = name;
        InventoryNumber = inventoryNumber;
    }

    public string GetInventoryNumber()
    {
        return InventoryNumber;
    }
}