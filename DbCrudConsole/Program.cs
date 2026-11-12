using Microsoft.Extensions.Configuration;
using DbCrudConsole;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

string cs = config.GetConnectionString("Default")!;

Console.WriteLine("Выберите способ доступа к БД:");
Console.WriteLine("1 - ADO.NET");
Console.WriteLine("2 - Entity Framework");
Console.Write("> ");
string? choice = Console.ReadLine();

IGameRepository repo = choice == "1" ? new AdoGameRepository(cs) : new EfGameRepository(cs);
Console.WriteLine(choice == "1" ? "Используется ADO.NET" : "Используется Entity Framework");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1 - Список игр");
    Console.WriteLine("2 - Добавить");
    Console.WriteLine("3 - Найти по ID");
    Console.WriteLine("4 - Изменить");
    Console.WriteLine("5 - Удалить");
    Console.WriteLine("0 - Выход");
    Console.Write("> ");

    string? command = Console.ReadLine();
    if (command == "0") break;

    try
    {
        switch (command)
        {
            case "1":
                var games = repo.GetAll();
                if (games.Count == 0) Console.WriteLine("Список пуст.");
                games.ForEach(PrintGame);
                break;

            case "2":
                var newGame = new Game
                {
                    Title = ReadString("Название: "),
                    Description = ReadOptionalString("Описание (можно пусто): "),
                    Price = ReadDecimal("Цена: "),
                    ReleaseDate = ReadOptionalDate("Дата выхода (гггг-мм-дд, можно пусто): "),
                    IsAvailable = ReadBool("В продаже? (y/n): ")
                };
                repo.Create(newGame);
                Console.WriteLine("Добавлено.");
                break;

            case "3":
                var found = repo.GetById(ReadInt("ID: "));
                if (found == null) Console.WriteLine("Не найдено.");
                else PrintGame(found);
                break;

            case "4":
                var toUpdate = repo.GetById(ReadInt("ID: "));
                if (toUpdate == null)
                {
                    Console.WriteLine("Не найдено.");
                    break;
                }
                PrintGame(toUpdate);
                toUpdate.Title = ReadString("Новое название: ");
                toUpdate.Description = ReadOptionalString("Новое описание (можно пусто): ");
                toUpdate.Price = ReadDecimal("Новая цена: ");
                toUpdate.ReleaseDate = ReadOptionalDate("Новая дата выхода (гггг-мм-дд, можно пусто): ");
                toUpdate.IsAvailable = ReadBool("В продаже? (y/n): ");
                repo.Update(toUpdate);
                Console.WriteLine("Изменено.");
                break;

            case "5":
                repo.Delete(ReadInt("ID: "));
                Console.WriteLine("Удалено (если запись существовала).");
                break;

            default:
                Console.WriteLine("Неизвестная команда.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static void PrintGame(Game g)
{
    string date = g.ReleaseDate?.ToString("yyyy-MM-dd") ?? "-";
    string available = g.IsAvailable ? "да" : "нет";
    Console.WriteLine($"[{g.GameID}] {g.Title} | {g.Price} | выход: {date} | в продаже: {available} | {g.Description}");
}

static string ReadString(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? s = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
        Console.WriteLine("Значение не может быть пустым.");
    }
}

static string? ReadOptionalString(string prompt)
{
    Console.Write(prompt);
    string? s = Console.ReadLine();
    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

static int ReadInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int value)) return value;
        Console.WriteLine("Введите целое число.");
    }
}

static decimal ReadDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? s = Console.ReadLine()?.Replace(',', '.');
        if (decimal.TryParse(s, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal value)
            && value >= 0)
            return value;
        Console.WriteLine("Введите неотрицательное число.");
    }
}

static DateTime? ReadOptionalDate(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParse(s, out DateTime value)) return value;
        Console.WriteLine("Неверный формат даты.");
    }
}

static bool ReadBool(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? s = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (s is "y" or "д" or "да" or "yes") return true;
        if (s is "n" or "н" or "нет" or "no") return false;
        Console.WriteLine("Введите y или n.");
    }
}
