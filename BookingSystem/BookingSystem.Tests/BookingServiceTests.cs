using BookingSystem.Tests.FakeClasses;
using BookingSystem.Services;
using BookingSystem.Models;

[TestClass]
public class BookingServiceTests
{
    private FakeEventRepository _eventRepository = null!;
    private FakeBookingRepository _bookingRepository = null!;
    private FakePaymentService _paymentService = null!;
    private FakeClock _clock = null!;
    private FakeBookingIdGenerator _idGenerator = null!;
    private BookingService _bookingService = null!;

    [TestInitialize]
    public void Setup()
    {
        _eventRepository = new FakeEventRepository();
        _bookingRepository = new FakeBookingRepository();
        _paymentService = new FakePaymentService();
        _clock = new FakeClock();
        _idGenerator = new FakeBookingIdGenerator();

        _bookingService = new BookingService(
            _eventRepository, _bookingRepository, _paymentService, _clock, _idGenerator);
    }

    [TestMethod]
    public void CreateBooking_ValidRequest_ReturnsConfirmedBooking()
    {
        _eventRepository.SetEvent(1, 10);
        _paymentService.ChargeResult = true;

        var fixedTime = new DateTimeOffset(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);
        _clock.UtcNow = fixedTime;

        var request = new BookingRequest(
            1,
            "testmail@gmail.com",
            5,
            20.0m
        );

        Booking result = _bookingService.CreateBooking(request);

        Assert.AreEqual(1, result.EventId);
        Assert.AreEqual(5, result.SeatsCount);
        Assert.AreEqual("testmail@gmail.com", result.CustomerEmail);
        Assert.AreEqual(5 * 20.0m, result.TotalAmount);
        Assert.AreEqual(BookingStatus.Confirmed, result.Status);

        Assert.AreEqual(5, _eventRepository.GetAvailableSeats(1));
        Assert.AreEqual(5 * 20.0m, _paymentService.LastChargedAmount);

        Assert.AreNotEqual(Guid.Empty, result.Id);
        Assert.AreEqual(fixedTime, result.CreatedAt);
        Assert.AreEqual(1, _paymentService.ChargeCallCount);
        Assert.AreEqual(1, _eventRepository.ReserveSeatsCallCount);
        Assert.AreEqual(0, _eventRepository.ReleaseSeatsCallCount);

        Assert.HasCount(1, _bookingRepository.Bookings);
    }

    [TestMethod]
    public void CreateBooking_PaymentFails_ReleasesReservedSeats()
    {
        _eventRepository.SetEvent(1, 10);
        _paymentService.ChargeResult = false;

        var request = new BookingRequest(
            1,
            "testmail@gmail.com",
            5,
            20.0m
        );

        Booking result = _bookingService.CreateBooking(request);

        Assert.AreEqual(BookingStatus.PaymentFailed, result.Status);
        Assert.HasCount(1, _bookingRepository.Bookings);
        Assert.AreEqual(1, _eventRepository.ReserveSeatsCallCount);
        Assert.AreEqual(1, _paymentService.ChargeCallCount);
        Assert.AreEqual(1, _eventRepository.ReleaseSeatsCallCount);
        Assert.AreEqual(10, _eventRepository.GetAvailableSeats(1));
    }

    [TestMethod]
    public void CreateBooking_PaymentThrowsException_ReleasesSeatsAndDoesNotSaveBooking()
    {
        _eventRepository.SetEvent(1, 10);
        _paymentService.ExceptionToThrow = new InvalidOperationException("Payment gateway error");

        var request = new BookingRequest(1, "testmail@gmail.com", 5, 20.0m);

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => _bookingService.CreateBooking(request));

        Assert.AreSame(_paymentService.ExceptionToThrow, exception);
        Assert.AreEqual(1, _eventRepository.ReserveSeatsCallCount);
        Assert.AreEqual(1, _paymentService.ChargeCallCount);
        Assert.AreEqual(1, _eventRepository.ReleaseSeatsCallCount);
        Assert.AreEqual(10, _eventRepository.GetAvailableSeats(1));
        Assert.HasCount(0, _bookingRepository.Bookings);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(11)]
    [DataRow(100)]
    public void CreateBooking_InvalidSeatsCount_ThrowsArgumentOutOfRangeException(int seatsCount)
    {
        _eventRepository.SetEvent(1, 10);

        var request = new BookingRequest(
            1,
            "testmail@gmail.com", 
            seatsCount, 
            20.0m
        );

        Assert.Throws<ArgumentOutOfRangeException>(() => _bookingService.CreateBooking(request));

        Assert.AreEqual(0, _eventRepository.ReserveSeatsCallCount);
        Assert.AreEqual(0, _eventRepository.ReleaseSeatsCallCount);
        Assert.AreEqual(0, _paymentService.ChargeCallCount);
        Assert.HasCount(0, _bookingRepository.Bookings);
    }

    [TestMethod]
    public void CreateBooking_EventDoesNotExist_ThrowsInvalidOperationException()
    {
        var request = new BookingRequest(1, "testmail@gmail.com", 2, 20.0m);

        Assert.Throws<InvalidOperationException>(() => _bookingService.CreateBooking(request));

        Assert.AreEqual(0, _eventRepository.ReserveSeatsCallCount);
        Assert.AreEqual(0, _paymentService.ChargeCallCount);
        Assert.HasCount(0, _bookingRepository.Bookings);
    }

    [TestMethod]
    public void CreateBooking_NotEnoughSeats_ThrowsInvalidOperationException()
    {
        _eventRepository.SetEvent(1, 3);

        var request = new BookingRequest(1, "testmail@gmail.com", 5, 20.0m);

        Assert.Throws<InvalidOperationException>(() => _bookingService.CreateBooking(request));

        Assert.AreEqual(0, _eventRepository.ReserveSeatsCallCount);
        Assert.AreEqual(0, _paymentService.ChargeCallCount);
        Assert.HasCount(0, _bookingRepository.Bookings);
    }

    [TestMethod]
    [DataRow(1, 100, 100)]
    [DataRow(2, 500, 1000)]
    [DataRow(3, 1250, 3750)]
    public void CreateBooking_ValidRequest_CalculatesCorrectTotalAmount(int seatsCount, int pricePerSeat, int expectedTotal)
    {
        _eventRepository.SetEvent(1, 10);
        _paymentService.ChargeResult = true;

        var request = new BookingRequest(1, "testmail@gmail.com", seatsCount, pricePerSeat);

        Booking result = _bookingService.CreateBooking(request);

        Assert.AreEqual((decimal)expectedTotal, result.TotalAmount);
    }
}