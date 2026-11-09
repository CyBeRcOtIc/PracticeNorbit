namespace StoreAnalytics.Models;

public sealed record OrderItem(
    int ProductId,
    int Quantity,
    decimal UnitPrice);