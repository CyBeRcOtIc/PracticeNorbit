namespace BookingSystem.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}