public class ExpertiseServiceCenter : ServiceCenter
{
    public bool Inspect(Vehicle vehicle)
    {
        return vehicle.Condition >= 70
               && vehicle.BrakesWork;
    }
}