using BookingSystem.Interfaces;

namespace BookingSystem.Tests.FakeClasses;

public sealed class FakePaymentService : IPaymentService
{
    public bool ChargeResult { get; set; }
    public Exception? ExceptionToThrow { get; set; }

    public int ChargeCallCount { get; private set; }
    public decimal LastChargedAmount { get; private set; }

    public bool Charge(string customerEmail, decimal amount)
    {
        ChargeCallCount++;
        LastChargedAmount = amount;

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return ChargeResult;
    }
}