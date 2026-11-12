using BookingSystem.Interfaces;
using BookingSystem.Models;

namespace BookingSystem.Tests.FakeClasses;

public sealed class FakeBookingRepository : IBookingRepository
{
    public List<Booking> Bookings { get; } = new();
    public void Save(Booking booking)
    {
        Bookings.Add(booking);
    }
}