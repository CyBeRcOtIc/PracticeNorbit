namespace StoreAnalytics.Models;

public sealed record Order(
int Id,
int CustomerId,
DateTime CreatedAt,
IReadOnlyList<OrderItem> Items);