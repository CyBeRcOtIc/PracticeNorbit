namespace BookingSystem.Models;

public sealed record BookingRequest(
    int EventId,
    string CustomerEmail,
    int SeatsCount,
    decimal PricePerSeat);