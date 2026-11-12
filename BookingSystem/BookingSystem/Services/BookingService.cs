using BookingSystem.Interfaces;
using BookingSystem.Models;

namespace BookingSystem.Services;

public sealed class BookingService
{
    public BookingService(
        IEventRepository eventRepository,
        IBookingRepository bookingRepository,
        IPaymentService paymentService,
        IClock clock,
        IBookingIdGenerator idGenerator)
    {
        EventRepository = eventRepository;
        BookingRepository = bookingRepository;
        PaymentService = paymentService;
        Clock = clock;
        BookingIdGenerator = idGenerator;
    }

    public Booking CreateBooking(BookingRequest request)
    {
        BaseValidation(request);

        decimal totalAmount = request.SeatsCount * request.PricePerSeat;

        Guid id = BookingIdGenerator.Create();
        DateTimeOffset createdAt = Clock.UtcNow;
        EventRepository.ReserveSeats(request.EventId, request.SeatsCount);
        
        bool status = false;

        try
        {
            status = PaymentService.Charge(request.CustomerEmail, totalAmount);
        }
        catch(Exception)
        {
            EventRepository.ReleaseSeats(request.EventId, request.SeatsCount);
            throw;
        }

        if(status)
        {
            Booking booking = new Booking(
                id,
                request.EventId,
                request.CustomerEmail,
                request.SeatsCount,
                totalAmount,
                createdAt,
                BookingStatus.Confirmed
            );

            BookingRepository.Save(booking);
            return booking;
        }
        else
        {
            EventRepository.ReleaseSeats(request.EventId, request.SeatsCount);

            Booking booking = new Booking(
                id,
                request.EventId,
                request.CustomerEmail,
                request.SeatsCount,
                totalAmount,
                createdAt,
                BookingStatus.PaymentFailed
            );

            BookingRepository.Save(booking);
            return booking;
        }
    }

    private void BaseValidation(BookingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if(request.EventId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "request.EventId not greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            throw new ArgumentException($"request.CustomerEmail null or white space");
        }

        if(!request.CustomerEmail.Contains("@"))
        {
            throw new ArgumentException($"request.CustomerEmail not contains '@', current value: {request.CustomerEmail}");
        }

        if(request.SeatsCount < 1 || request.SeatsCount > 10)
        {
            throw new ArgumentOutOfRangeException("request.SeatsCount not within the range");
        }

        if(request.PricePerSeat <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), $"PricePerSeat is not greater than zero");
        }

        if (!EventRepository.Exists(request.EventId))
        {
            throw new InvalidOperationException($"Event with id {request.EventId} does not exist.");
        }

        int availableSeats = EventRepository.GetAvailableSeats(request.EventId);

        if (availableSeats < request.SeatsCount)
        {
            throw new InvalidOperationException(
                $"Not enough available seats. Requested: {request.SeatsCount}, available: {availableSeats}.");
        }
    }

    private IEventRepository EventRepository { get; init; }

    private IBookingRepository BookingRepository { get; init; }

    private IPaymentService PaymentService { get; init; }

    private IClock Clock { get; init; }

    private IBookingIdGenerator BookingIdGenerator { get; init; }
}