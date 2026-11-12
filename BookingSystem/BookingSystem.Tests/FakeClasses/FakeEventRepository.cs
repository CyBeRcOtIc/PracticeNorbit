using BookingSystem.Interfaces;

namespace BookingSystem.Tests.FakeClasses;

public sealed class FakeEventRepository : IEventRepository
{
    private readonly Dictionary<int, int> eventSeats = new();

    public int ReserveSeatsCallCount { get; private set; }
    public int ReleaseSeatsCallCount { get; private set; }

    public void SetEvent(int eventId, int availableSeats)
    {
        eventSeats[eventId] = availableSeats;
    }

    public bool Exists(int eventId)
    {
        return eventSeats.ContainsKey(eventId);
    }

    public int GetAvailableSeats(int eventId)
    {
        return eventSeats[eventId];
    }

    public void ReserveSeats(int eventId, int seatsCount)
    {
        ReserveSeatsCallCount++;
        eventSeats[eventId] -= seatsCount;
    }

    public void ReleaseSeats(int eventId, int seatsCount)
    {
        ReleaseSeatsCallCount++;
        eventSeats[eventId] += seatsCount;
    }
}