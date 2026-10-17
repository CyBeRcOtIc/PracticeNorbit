public sealed class OrderProcessedEventArgs : EventArgs
{
    public OrderProcessedEventArgs(
        int orderId,
        decimal total,
        int processedItemsCount,
        DateTimeOffset processedAt)
    {
        OrderId = orderId;
        Total = total;
        ProcessedItemsCount = processedItemsCount;
        ProcessedAt = processedAt;
    }

    public int OrderId { get; }
    public decimal Total { get; }
    public int ProcessedItemsCount { get; }
    public DateTimeOffset ProcessedAt { get; }
}