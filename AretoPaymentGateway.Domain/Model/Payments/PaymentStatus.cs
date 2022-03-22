namespace AretoPaymentGateway.Domain.Model.Payments
{
    public enum PaymentStatus
    {
        Received = 0,
        Authorized = 1,
        Settled = 2,
        Canceled = 3
    }
}
