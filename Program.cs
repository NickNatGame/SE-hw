using System.Globalization;
using CPO.Domain;
using CPO.Interfaces;
using CPO.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CPO;

public static class Program
{
    public static void Main()
    {
        /* Создаем список инструкций, какие обьекты контейнер должен уметь создавать*/
        ServiceCollection services = new ServiceCollection();

        /* Добавляем инструкцию, что если нужен ServiceCenter -> используется ExpertiseServiceCenter */
        services.AddSingleton<IServiceCenter, ExpertiseServiceCenter>();

        /* Добавляем сам Service */
        services.AddSingleton<Service>();

        /* Это наш DI-контейнер, который будет выполнять наши инструкции ( using var значит, что мы в конце выполнения программы этот контейнер освобождаем ) */
        using ServiceProvider provider = services.BuildServiceProvider();

        /* Просим DI-контейнер выдать Service */
        Service service = provider.GetRequiredService<Service>();


        Console.WriteLine("ВышКат\n");
        Console.WriteLine("1 - использовать тестовые примеры");
        Console.WriteLine("2 - ввести свои транспорт и вещи");
        int mode = ReadInt("Выберите вариант: ", 1, 2);

        List<Vehicle> vehicles = new List<Vehicle>();
        List<Thing> things = new List<Thing>();

        if (mode == 1)
        {
            AddDemoItems(vehicles, things);
        }
        else
        {
            ReadItems(vehicles, things);
        }

        Console.WriteLine("\nПриёмка транспорта:");
        foreach (Vehicle vehicle in vehicles)
        {
            bool accepted = service.AddVehicle(vehicle);
            string result;

            if (accepted)
            {
                result = "принят в парк";
            }
            else
            {
                result = "отклонён техосмотром";
            }

            Console.WriteLine($"{vehicle.Name} ({vehicle.InventoryNumber}): {result}");
        }

        Console.WriteLine("\nДобавление вещей:");
        foreach (Thing thing in things)
        {
            service.AddThing(thing);
            Console.WriteLine($"{thing.Name} ({thing.InventoryNumber}): добавлено");
        }

        PrintReport(service);
    }

    private static void AddDemoItems(List<Vehicle> vehicles, List<Thing> things)
    {
        vehicles.Add(new Scooter("Городской самокат", "G67", 5, 85, true, 2.5));
        vehicles.Add(new Scooter("Спортивный самокат", "I228", 8, 90, true, 3.5));
        vehicles.Add(new Bike("Городской велосипед", "J42", 4, 80, true));
        vehicles.Add(new Scooter("Изношенный самокат", "K900", 9, 50, true, 2.0));
        vehicles.Add(new Bike("Велосипед без тормозов", "B52", 7, 90, false));

        things.Add(new Helmet("Шлем", "US1"));
        things.Add(new ChargingStation("Док-станция", "YA2", 10.0));
    }

    private static void ReadItems(List<Vehicle> vehicles, List<Thing> things)
    {
        while (true)
        {
            Console.WriteLine("\n1 - самокат");
            Console.WriteLine("2 - велосипед");
            Console.WriteLine("3 - шлем");
            Console.WriteLine("4 - док-станция");
            Console.WriteLine("0 - закончить ввод и показать отчёт");

            int choice = ReadInt("Выберите тип объекта: ", 0, 4);

            if (choice == 0)
            {
                return;
            }

            string name = ReadText("Название: ");
            string inventoryNumber = ReadInventoryNumber(vehicles, things);

            switch (choice)
            {
                case 1:
                case 2:
                    {
                        int beginnerEase = ReadInt("Простота для новичка (1–10): ", 1, 10);
                        int condition = ReadInt("Состояние (0–100): ", 0, 100);
                        bool brakesWork =
                            ReadInt("Тормоза исправны? (1 - да, 0 - нет): ", 0, 1) == 1;

                        if (choice == 1)
                        {
                            double energy = ReadEnergy();
                            vehicles.Add(new Scooter(
                                name, inventoryNumber, beginnerEase, condition, brakesWork, energy));
                        }
                        else
                        {
                            vehicles.Add(new Bike(
                                name, inventoryNumber, beginnerEase, condition, brakesWork));
                        }

                        Console.WriteLine(
                            "\nПосле завершения ввода транспорт пройдёт техосмотр " +
                            "и при успешной проверке будет добавлен в парк");
                        break;
                    }

                case 3:
                    {
                        things.Add(new Helmet(name, inventoryNumber));
                        Console.WriteLine("Шлем после завершения ввода будет добавлен");
                        break;
                    }

                case 4:
                    {
                        double energy = ReadEnergy();
                        things.Add(new ChargingStation(name, inventoryNumber, energy));
                        Console.WriteLine("Зарядная станция после завершения ввода будет добавлена");
                        break;
                    }
            }
        }
    }

    private static string ReadInventoryNumber(List<Vehicle> vehicles, List<Thing> things)
    {
        while (true)
        {
            string inventoryNumber = ReadText("Уникальный ID: ");
            bool numberExists = false;

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.InventoryNumber == inventoryNumber)
                {
                    numberExists = true;
                    break;
                }
            }

            foreach (Thing thing in things)
            {
                if (thing.InventoryNumber == inventoryNumber)
                {
                    numberExists = true;
                    break;
                }
            }

            if (!numberExists)
            {
                return inventoryNumber;
            }

            Console.WriteLine("Этот ID уже используется, введите другой ID");
        }
    }

    private static void PrintReport(Service service)
    {
        List<IInventoryItem> inventory = service.GetInventory();
        int vehicleCount = 0;
        int thingCount = 0;

        foreach (IInventoryItem item in inventory)
        {
            if (item is Vehicle)
            {
                vehicleCount++;
            }
            else if (item is Thing)
            {
                thingCount++;
            }
        }

        Console.WriteLine("\nОтчёт по парку:");
        Console.WriteLine($"Транспортных средств: {vehicleCount}");
        Console.WriteLine($"Вещей: {thingCount}");
        Console.WriteLine($"Всего объектов на балансе: {inventory.Count}");
        Console.WriteLine($"Суммарное суточное потребление энергии: {service.GetTotalEnergyConsumption():F2} кВт·ч");

        Console.WriteLine("\nТехника для новичков (простота >= 6):");
        List<Vehicle> beginnerVehicles = service.GetVehiclesForBeginners();

        if (beginnerVehicles.Count == 0)
        {
            Console.WriteLine("Подходящей техники нет");
        }
        else
        {
            foreach (Vehicle vehicle in beginnerVehicles)
            {
                Console.WriteLine($"{vehicle.Name} ({vehicle.InventoryNumber}), простота: {vehicle.BeginnerEase}");
            }
        }

        Console.WriteLine("\nВесь инвентарь:");
        foreach (IInventoryItem item in inventory)
        {
            Console.WriteLine($"{item.Name}, ID: {item.InventoryNumber}");
        }
    }

    private static string ReadText(string prompt)
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

    private static int ReadInt(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            string input = ReadText(prompt);
            bool isNumber = int.TryParse(input, out int value);

            if (isNumber && value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Введите целое число от {minimum} до {maximum}");
        }
    }

    private static double ReadEnergy()
    {
        while (true)
        {
            string input = ReadText("Суточный расход энергии (кВт·ч): ");
            input = input.Replace(',', '.');
            bool isNumber = double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double energy);

            if (isNumber && double.IsFinite(energy) && energy >= 0)
            {
                return energy;
            }

            Console.WriteLine("Введите неотрицательное число, например 2,5 или 2.5");
        }
    }
}
