using System;
using System.Text;

string DiamondPrint(int n)
{
    if(n <= 0)
        return "Error: The number must be greater than zero!!!";
    else if(n % 2 == 0)
        return "Error: The number must be odd!!!";

    int center = n / 2;
    var sb = new StringBuilder();

    for (int row = 0; row < n; row++)
    {
        int distance = Math.Abs(row - center);
        int left = distance;
        int right = n - 1 - distance;

        for (int col = 0; col < n; col++)
        {
            if(col == right || col == left)
            {
                sb.Append("x");
            }
            else
            {
                sb.Append(" ");
            }
        }

        sb.AppendLine();
    }
    
    return sb.ToString().TrimEnd();
}

Console.WriteLine(DiamondPrint(5));