using BookingSystem.Interfaces;

namespace BookingSystem.Tests.FakeClasses;

public sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow {get; set;}
}