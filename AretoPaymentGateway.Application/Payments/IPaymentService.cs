using System.Threading.Tasks;

namespace AretoPaymentGateway.Application.Payments
{
    public interface IPaymentService
    {
        Task<int> AuthorizePaymentAsync(INewPayment newPayment);
        Task CapturePaymentAsync(int paymentId, decimal amount, string currency);
        Task CancelPaymentAsync(int paymentId);
    }
}
