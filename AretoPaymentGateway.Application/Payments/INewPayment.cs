namespace AretoPaymentGateway.Application.Payments
{
    public interface INewPayment
    {
        public decimal Amount { get; }
        public string Currency { get; }
        public string Reference { get; }
        public string Type { get; }
        public string CardNumber { get; }
        public string ExpiryMonth { get; }
        public string ExpiryYear { get; }
        public string SecurityCode { get; }
    }
}
