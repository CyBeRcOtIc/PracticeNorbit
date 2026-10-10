public class Product
{
    private string _name = null!;
    private string _manufacturer = null!;
    private decimal _price;

    public decimal Price 
    {
        get => _price; 
        set
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The price cannot be negative.");
            }

            _price = value;
        }
    }
    
    public string Name 
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The name cannot be null.", nameof(value));
            }

            _name = value;
        }
    }
    
    public string Manufacturer
    {
        get => _manufacturer;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The manufacturer cannot be null.", nameof(value));
            }

            _manufacturer = value;
        }
    }
    
    public DateTime ProductionDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public Product(string name, string manufacturer, decimal price,
    DateTime productionDate, DateTime endDate)
    {
        if (endDate < productionDate)
        {
            throw new ArgumentException("The expiration date cannot be earlier than the production date.");
        }

        Name = name;
        Manufacturer = manufacturer;
        Price = price;
        SetDates(productionDate, endDate);
    }

    /// <summary>
    /// Sets the production date and the expiration date at the same time.
    /// </summary>
    public void SetDates(DateTime productionDate, DateTime endDate)
    {
        if (endDate < productionDate)
        {
            throw new ArgumentException(
                "The expiration date cannot be earlier than the production date.");
        }

        ProductionDate = productionDate;
        EndDate = endDate;
    }

    public override string ToString()
    {
        return $"{Name} ({Manufacturer}), price: {Price:F2}, " +
               $"produced: {ProductionDate:dd.MM.yyyy}, " +
               $"expires: {EndDate:dd.MM.yyyy}";
    }
}