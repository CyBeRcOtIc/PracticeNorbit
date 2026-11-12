namespace BookingSystem.Models;

public sealed record Booking(
    Guid Id,
    int EventId,
    string CustomerEmail,
    int SeatsCount,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    BookingStatus Status);