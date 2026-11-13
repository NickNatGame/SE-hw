using CPO.Domain;
using CPO.Interfaces;

namespace CPO.Services;

public class ExpertiseServiceCenter : IServiceCenter
{
    public bool Inspect(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            throw new ArgumentNullException(nameof(vehicle));
        }

        /*
            Добавил свою реализацию проверки на техосмотре, в enterprice должен быть подключена какая-то библиотека с техосмотром 
            или реализована какая-либо метрика прохода/непрохода техосмотра
        */

        return vehicle.Condition >= 70 && vehicle.BrakesWork;
    }
}
