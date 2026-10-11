namespace CPO.Domain;

public class Bike : Vehicle
{
    public Bike(
        string name,
        string inventoryNumber,
        int beginnerEase,
        int condition,
        bool brakesWork)
        : base(name, inventoryNumber, beginnerEase, condition, brakesWork)
    {
    }
}
