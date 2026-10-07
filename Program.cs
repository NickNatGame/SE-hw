using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

/* Создаем список инструкций, какие обьекты контейнер должен уметь создавать*/
var services = new ServiceCollection();

/* Добавляем инструкцию, что если нужен ServiceCenter -> используется ExpertiseServiceCenter */
services.AddSingleton<ServiceCenter, ExpertiseServiceCenter>();

/* Добавляем сам Service */
services.AddSingleton<Service>();

/* Это наш DI-контейнер, который будет выполнять наши инструкции ( using var значит, что мы в конце выполнения программы этот контейнер освобождаем ) */
using var provider = services.BuildServiceProvider();

/* Просим DI-контейнер выдать Service */ 
var service = provider.GetRequiredService<Service>();

Console.WriteLine("ВышКат\n");

Console.WriteLine("1 - использовать тестовые примеры");
Console.WriteLine("2 - ввести свои транспорт и вещи");
int mode = ReadInt("Выберите вариант: ", 1, 2);

var vehicles = new List<Vehicle>();
var things = new List<Thing>();

if (mode == 1)
{
    vehicles.AddRange(new Vehicle[]
    {
    new Scooter("Городской самокат", "G67", 5, 85, true, 2.5),
    new Scooter("Спортивный мега крутой самокат", "I228", 8, 90, true, 3.5),
    new Bike("Городской велосипед", "J42", 4, 80, true),
    new Scooter("Изношенный самокат", "K900", 9, 50, true, 2.0),
    new Bike("Велосипед без тормозов", "B52", 7, 90, false)
    });

    things.Add(new Helmet("Шлем", "US1"));
    things.Add(new ChargingStation("Док-станция", "YA2", 10.0));
}
else
{
    while (true)
    {
        Console.WriteLine("\n1 - самокат");
        Console.WriteLine("2 - велосипед");
        Console.WriteLine("3 - шлем");
        Console.WriteLine("4 - Док-станция");
        Console.WriteLine("0 - закончить ввод и показать отчёт");
        int choice = ReadInt("Выберите тип объекта: ", 0, 4);

        if (choice == 0)
        {
            break;
        }

        string name = ReadText("Название: ");
        string inventoryNumber;

        while (true)
        {
            inventoryNumber = ReadText("Уникальный ID: ");
            bool numberExists = false;

            foreach (var vehicle in vehicles)
            {
                if (vehicle.InventoryNumber == inventoryNumber)
                {
                    numberExists = true;
                    break;
                }
            }
            foreach (var thing in things)
            {
                if (thing.InventoryNumber == inventoryNumber)
                {
                    numberExists = true;
                    break;
                }
            }

            if (!numberExists)
            {
                break;
            }

            Console.WriteLine("Этот ID уже используется, введите другой ID");
        }

        if (choice == 1 || choice == 2)
        {
            int level = ReadInt("Необходимый уровень вождения (1–10): ", 1, 10);
            int condition = ReadInt("Состояние (0–100): ", 0, 100);
            bool brakesWork = ReadInt("Тормоза исправны? (1 - да, 0 - нет): ", 0, 1) == 1;

            if (choice == 1)
            {
                double energy = ReadEnergy();
                vehicles.Add(new Scooter(name, inventoryNumber, level, condition, brakesWork, energy));
            }
            else
            {
                vehicles.Add(new Bike(name, inventoryNumber, level, condition, brakesWork));
            }

            Console.WriteLine("\nТранспорт после завершения ввода будет проверен на тех осмотре и, при успешной проверки, будет добавлен в парк");
        }
        else if (choice == 3)
        {
            things.Add(new Helmet(name, inventoryNumber));
            Console.WriteLine("Шлем после завершения ввода будет добавлен");
        }
        else
        {
            double energy = ReadEnergy();
            things.Add(new ChargingStation(name, inventoryNumber, energy));
            Console.WriteLine("Зарядная станция после завершения ввода будет добавлен");
        }
    }
}

Console.WriteLine("\nПриёмка транспорта:");

foreach (var vehicle in vehicles)
{
    bool accepted = service.addVehicle(vehicle);
    string result = "отклонён техосмотром";
    if (accepted)
    {
        result = "принят в парк";
    }
    Console.WriteLine($"{vehicle.Name} ({vehicle.InventoryNumber}): {result}");
}

Console.WriteLine("\nДобавление вещей:");
foreach (var thing in things)
{
    service.AddThing(thing);
    Console.WriteLine($"{thing.Name} ({thing.InventoryNumber}): добавлено");
}

var inventory = service.GetInventory();
int vehicleCount = inventory.OfType<Vehicle>().Count();
int thingCount = inventory.OfType<Thing>().Count();

Console.WriteLine("\nОтчёт по парку:");
Console.WriteLine($"Транспортных средств: {vehicleCount}");
Console.WriteLine($"Вещей: {thingCount}");
Console.WriteLine($"Всего объектов на балансе: {inventory.Count}");
Console.WriteLine($"Суммарное суточное потребление энергии: {service.getTotalEnergyConsumption():F2} кВт·ч");

Console.WriteLine("\nТехника для новичков (уровень ≥ 6):");
var beginnerVehicles = service.GetVehiclesForBeginners();
if (beginnerVehicles.Count == 0)
{
    Console.WriteLine("Подходящей техники нет");
}
else
{
    foreach (var vehicle in beginnerVehicles)
    {
        Console.WriteLine($"{vehicle.Name} ({vehicle.InventoryNumber}), уровень: {vehicle.Level}");
    }
}

Console.WriteLine("\nВесь инвентарь:");
foreach (var item in inventory)
{
    string name;
    if (item is Vehicle vehicle)
    {
        name = vehicle.Name;
    }
    else if (item is Thing thing)
    {
        name = thing.Name;
    }
    /* На всякий, если будем расширять наш класс. */
    else
    {
        name = item.GetType().Name;
    }

    Console.WriteLine($"{name}, id: {item.GetInventoryNumber()}");
}


static string ReadText(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (input == null)
        {
            Console.WriteLine("\nВвод завершён");
            Environment.Exit(0);
            return "";
        }

        input = input.Trim();
        if (input.Length > 0)
        {
            return input;
        }

        Console.WriteLine("Введите непустое значение");
    }
}

static int ReadInt(string prompt, int minimum, int maximum)
{
    while (true)
    {
        string input = ReadText(prompt);
        if (int.TryParse(input, out int value) && value >= minimum && value <= maximum)
        {
            return value;
        }

        Console.WriteLine($"Введите целое число от {minimum} до {maximum}.");
    }
}

static double ReadEnergy()
{
    while (true)
    {
        string input = ReadText("Суточный расход энергии (кВт·ч): ").Replace(',', '.');
        bool isNumber = double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double energy);

        if (isNumber)
        {
            if (double.IsFinite(energy))
            {
                if (energy >= 0)
                {
                    return energy;
                }
            }
        }

        Console.WriteLine("Введите неотрицательное число, например 2,5 или 2.5");
    }
}
