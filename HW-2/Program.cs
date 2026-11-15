using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;
using Pos.Presentation;

namespace Pos;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--demo")
        {
            using ServiceProvider demoProvider = CreateProvider(
                new WeightedScoreStrategy(), false, Console.In, Console.Out);
            demoProvider.GetRequiredService<ConsoleApplication>().RunDemo();
            return;
        }

        Console.WriteLine("ПОС - преподавательская оценка студентов");
        Console.WriteLine("Данные хранятся в памяти до выхода. Итоги можно экспортировать в CSV.");
        Console.WriteLine("1 - взвешенный итог, 2 - среднее долей выполнения (без весов)");
        int? formula = ReadChoice("Формула: ", 1, 2);
        if (formula == null)
        {
            return;
        }

        Console.WriteLine("0 - без округления; 1 - округление до сотых");
        int? rounding = ReadChoice("Округление: ", 0, 1);
        if (rounding == null)
        {
            return;
        }

        IFinalScoreStrategy strategy = new WeightedScoreStrategy();
        if (formula == 2)
        {
            strategy = new AverageScoreStrategy();
        }

        using ServiceProvider provider = CreateProvider(strategy, rounding == 1, Console.In, Console.Out);
        ConsoleApplication application = provider.GetRequiredService<ConsoleApplication>();
        application.Run();
    }

    public static ServiceProvider CreateProvider(
        IFinalScoreStrategy strategy, bool rounded, TextReader input, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<IStudentRepository, InMemoryStudentRepository>();
        services.AddSingleton<IRubricRepository, InMemoryRubricRepository>();
        services.AddSingleton<IFinalScoreExporter, CsvFinalScoreExporter>();
        services.AddSingleton<IFinalScoreStrategy>(strategy);
        services.AddSingleton<FinalScoreCalculator>();
        services.AddSingleton<IFinalScoreCalculator>(provider =>
        {
            IFinalScoreCalculator calculator = provider.GetRequiredService<FinalScoreCalculator>();
            if (rounded)
            {
                calculator = new RoundedFinalScoreCalculator(calculator);
            }

            return calculator;
        });
        services.AddSingleton<GradingService>();
        services.AddSingleton<TextReader>(input);
        services.AddSingleton<TextWriter>(output);
        services.AddSingleton<ConsoleApplication>();
        return services.BuildServiceProvider();
    }

    private static int? ReadChoice(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            Console.Write(prompt);
            string? text = Console.ReadLine();
            if (text == null)
            {
                return null;
            }

            if (int.TryParse(text, out int value) && value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Введите целое число от {minimum} до {maximum}.");
        }
    }
}
