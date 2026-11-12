namespace BookingSystem.Interfaces;

public interface IBookingIdGenerator
{
    Guid Create();
}