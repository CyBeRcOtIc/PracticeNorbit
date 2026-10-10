using System.Globalization;
using System.Text;

string CalcYearRates(decimal initialDeposit, int years, decimal interestRate)
{
    decimal currentDeposit = initialDeposit;
    decimal rate = interestRate / 100m;
    var sb = new StringBuilder();

    for (int i = 1; i <= years; i++)
    {
        currentDeposit *= (1 + rate);
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"Год {i}: {currentDeposit:F2} руб."));
    }

    return sb.ToString().TrimEnd();
}

Console.WriteLine(CalcYearRates(1000, 3, 10));