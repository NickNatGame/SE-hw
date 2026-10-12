using System.Globalization;

namespace CPO.Tests;

[CollectionDefinition("Console", DisableParallelization = true)]
public class ConsoleCollection
{
}

[Collection("Console")]
public class ConsoleApplicationTests
{
    private static string RunApplication(string input)
    {
        var originalInput = Console.In;
        var originalOutput = Console.Out;
        var originalCulture = CultureInfo.CurrentCulture;
        using var reader = new StringReader(input);
        using var writer = new StringWriter(CultureInfo.InvariantCulture);

        try
        {
            Console.SetIn(reader);
            Console.SetOut(writer);
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // Запускаем приложение так же, как при обычном запуске, но с готовым вводом
            Program.Main();
            return writer.ToString();
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
    [Fact]
    public void Demo_ShowsInspectionInventoryEnergyAndBeginnerReports()
    {
        string output = RunApplication("1\n");

        Assert.Contains("Городской самокат (G67): принят в парк", output);
        Assert.Contains("Изношенный самокат (K900): отклонён техосмотром", output);
        Assert.Contains("Велосипед без тормозов (B52): отклонён техосмотром", output);
        Assert.Contains("Транспортных средств: 3", output);
        Assert.Contains("Вещей: 2", output);
        Assert.Contains("Всего объектов на балансе: 5", output);
        Assert.Contains("Суммарное суточное потребление энергии: 16.00 кВт·ч", output);
        Assert.Contains("Спортивный самокат (I228), простота: 8", output);
        Assert.Contains("Шлем, ID: US1", output);
        Assert.Contains("Док-станция, ID: YA2", output);
    }

    [Fact]
    public void CustomInput_AddsEveryItemTypeAndRetriesInvalidValues()
    {
        string input = string.Join('\n', new[]
        {
            "wrong", "3", "2",                 // Режим: не число, вне диапазона, затем ручной ввод
            "1", " ", "Самокат", " ", "S1",    // Непустые название и ID
            "0", "11", "6",                    // Уровень для новичка, две ошибки, затем граница для новичков
            "-1", "101", "70",                 // Состояние, две ошибки, затем граница техосмотра
            "2", "1",                          // Тормоза, ошибка, затем исправны
            "NaN", "Infinity", "-1", "2,5",    // Энергия, три ошибки, затем десятичная запятая
            "2", "Велосипед", "S1", "B1",      // Повтор номера транспорта, затем уникальный ID
            "5", "100", "1",
            "3", "Шлем", "H1",
            "4", "Станция", "H1", "C1",        // Повтор номера вещи, затем уникальный ID
            "10.5",                            // Десятичная точка
            "0"
        }) + "\n";

        string output = RunApplication(input);

        Assert.Contains("Введите целое число от 1 до 2", output);
        Assert.Contains("Введите непустое значение", output);
        Assert.Contains("Введите целое число от 1 до 10", output);
        Assert.Contains("Введите целое число от 0 до 100", output);
        Assert.Contains("Введите целое число от 0 до 1", output);
        Assert.Contains("Введите неотрицательное число", output);
        Assert.Equal(2, output.Split("Этот ID уже используется", StringSplitOptions.None).Length - 1);
        Assert.Contains("Транспортных средств: 2", output);
        Assert.Contains("Вещей: 2", output);
        Assert.Contains("Всего объектов на балансе: 4", output);
        Assert.Contains("Суммарное суточное потребление энергии: 13.00 кВт·ч", output);
        Assert.Contains("Самокат (S1), простота: 6", output);
        Assert.DoesNotContain("Велосипед (B1), простота: 5", output);
        Assert.Contains("Самокат, ID: S1", output);
        Assert.Contains("Велосипед, ID: B1", output);
        Assert.Contains("Шлем, ID: H1", output);
        Assert.Contains("Станция, ID: C1", output);
    }

    [Fact]
    public void EmptyInputSession_ShowsZeroReportsAndNoBeginnerVehicles()
    {
        string output = RunApplication("2\n0\n");

        Assert.Contains("Транспортных средств: 0", output);
        Assert.Contains("Вещей: 0", output);
        Assert.Contains("Всего объектов на балансе: 0", output);
        Assert.Contains("Суммарное суточное потребление энергии: 0.00 кВт·ч", output);
        Assert.Contains("Подходящей техники нет", output);
    }
}
