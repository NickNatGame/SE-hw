using CPO.Interfaces;

namespace CPO.Domain;

public abstract class Thing : IInventoryItem
{
    public string Name { get; }
    public string InventoryNumber { get; }

    protected Thing(string name, string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название не должно быть пустым", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(inventoryNumber))
        {
            throw new ArgumentException("ID не должен быть пустым", nameof(inventoryNumber));
        }

        Name = name.Trim();
        InventoryNumber = inventoryNumber.Trim();
    }
}