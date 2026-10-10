public class DiscountedProduct : Product
{
    private decimal _discountPercent;

    public DiscountedProduct(string name, string manufacturer, decimal price,
        DateTime productionDate, DateTime endDate, decimal discountPercent)
        : base(name, manufacturer, price, productionDate, endDate)
    {
        DiscountPercent = discountPercent;
    }

    public decimal DiscountPercent
    {
        get => _discountPercent;
        set
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), "The discount must be between 0 and 100.");
            }

            _discountPercent = value;
        }
    }
    
    public decimal PromoPrice => Price * (1 - DiscountPercent / 100m);

    public override string ToString()
    {
        return $"{base.ToString()}, discount: {DiscountPercent:F2}%, " +
               $"promo price: {PromoPrice:F2}";
    }
}