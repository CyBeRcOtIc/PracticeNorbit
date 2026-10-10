using System.Globalization;

string ReadNonEmptyString(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }

        Console.WriteLine("The value cannot be empty. Try again.");
    }
}

decimal ReadPrice(string prompt)
{
    while (true)
    {
        Console.Write(prompt);

        if (decimal.TryParse(Console.ReadLine(), out decimal result) && result >= 0)
        {
            return result;
        }

        Console.WriteLine("Enter a non-negative number (for example, 89,90).");
    }
}

decimal ReadPercent(string prompt)
{
    while (true)
    {
        Console.Write(prompt);

        if (decimal.TryParse(Console.ReadLine(), out decimal result)
            && result >= 0 && result <= 100)
        {
            return result;
        }

        Console.WriteLine("Enter a number from 0 to 100.");
    }
}

DateTime ReadDate(string prompt, DateTime minDate)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (!DateTime.TryParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime result))
        {
            Console.WriteLine("Invalid format. Use dd.MM.yyyy, for example 10.10.2026.");
            continue;
        }

        if (result < minDate)
        {
            Console.WriteLine($"The date cannot be earlier than {minDate:dd.MM.yyyy}.");
            continue;
        }

        return result;
    }
}

Console.WriteLine("Product Creation\n");

string name = ReadNonEmptyString("Enter the product name: ");
string manufacturer = ReadNonEmptyString("Enter the manufacturer: ");

decimal price = ReadPrice("Enter the price: ");
decimal discountPercent = ReadPercent("Enter the discountPercent: ");

DateTime productionDate = ReadDate("Enter the production date (dd.MM.yyyy): ", DateTime.MinValue);
DateTime endDate = ReadDate("Enter the expiration date (dd.MM.yyyy): ", productionDate);

var product = new DiscountedProduct(name, manufacturer, price, productionDate, endDate, discountPercent);

Console.WriteLine();
Console.WriteLine("DiscountedProduct created:");
Console.WriteLine(product);