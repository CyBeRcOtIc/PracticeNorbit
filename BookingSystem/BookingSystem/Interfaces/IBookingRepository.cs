using BookingSystem.Models;

namespace BookingSystem.Interfaces;

public interface IBookingRepository
{
    void Save(Booking booking);
}