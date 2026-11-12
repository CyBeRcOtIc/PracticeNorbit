namespace BookingSystem.Interfaces;

public interface IEventRepository
{
    bool Exists(int eventId);

    int GetAvailableSeats(int eventId);

    void ReserveSeats(int eventId, int seatsCount);

    void ReleaseSeats(int eventId, int seatsCount);
}