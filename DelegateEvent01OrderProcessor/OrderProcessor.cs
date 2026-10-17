public sealed class OrderProcessor
{
    public event EventHandler<OrderProcessedEventArgs>? OrderProcessed;

    
    public decimal Process(
        int orderId,
        IEnumerable<OrderItem> items,
        Predicate<OrderItem> shouldProcess,
        Func<decimal, decimal> transformPrice,
        ItemTotalCalculator calculateItemTotal,
        Action<string>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(shouldProcess);
        ArgumentNullException.ThrowIfNull(transformPrice);
        ArgumentNullException.ThrowIfNull(calculateItemTotal);

        if(orderId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(orderId), orderId, "The orderId must be positive.");
        }

        foreach (var item in items)
        {
            if (item.Price < 0)
            {
                throw new ArgumentException($"The product \"{item.Name}\" has a negative price.", nameof(items));
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException($"The product \"{item.Name}\" must have a positive quantity.", nameof(items));
            }
        }

        decimal orderCost = 0;
        int processedCount = 0;

        logger?.Invoke("process start");
        foreach (var item in items)
        {
            if(!shouldProcess(item))
            {
                logger?.Invoke($"The product \"{item.Name}\" was excluded");
                continue;
            }

            decimal transformedPrice = transformPrice(item.Price);
            logger?.Invoke($"New TransformedPrice at \"{item.Name}\": {transformedPrice}");

            decimal cost = calculateItemTotal(item, transformedPrice);
            logger?.Invoke($"the calculated cost of the item: {cost}");
            orderCost += cost;
            processedCount++;
        }
        logger?.Invoke("process end");

        OnOrderProcessed(new OrderProcessedEventArgs(
                                                    orderId,
                                                    orderCost, 
                                                    processedCount, 
                                                    DateTimeOffset.UtcNow));

        return orderCost;
    }

    private void OnOrderProcessed(OrderProcessedEventArgs e)
    {
        OrderProcessed?.Invoke(this, e);
    }
}