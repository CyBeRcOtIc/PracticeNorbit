using BookingSystem.Interfaces;
namespace BookingSystem.Tests.FakeClasses;

public sealed class FakeBookingIdGenerator : IBookingIdGenerator
{
    public Guid IdToReturn { get; set; } = Guid.NewGuid();

    public Guid Create() => IdToReturn;
}