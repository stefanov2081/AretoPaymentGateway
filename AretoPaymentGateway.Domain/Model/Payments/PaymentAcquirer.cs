using System.Threading.Tasks;

namespace AretoPaymentGateway.Domain.Model.Payments
{
    public abstract class PaymentAcquirer
    {
        public PaymentAcquirer(string name)
        {
            Name = name;
        }

        public string Name { get; init; }

        public abstract Task<string> AuthorizePaymentAsync(Payment payment);
        public abstract Task CapturePaymentAsync(Payment payment);
        public abstract Task CancelPaymentAsync(Payment payment);
    }
}
