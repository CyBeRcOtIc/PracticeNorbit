namespace BookingSystem.Interfaces;

public interface IPaymentService
{
    bool Charge(string customerEmail, decimal amount);
}