public class Bike : Vehicle
{
    public Bike(
        string name,
        string inventoryNumber,
        int level,
        int condition, 
        bool brakesWork
    ) : base(name, inventoryNumber, level, condition, brakesWork)
    {
        
    }
}