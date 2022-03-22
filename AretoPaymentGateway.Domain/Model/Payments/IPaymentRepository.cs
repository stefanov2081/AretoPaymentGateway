using System.Threading.Tasks;

namespace AretoPaymentGateway.Domain.Model.Payments
{
    public interface IPaymentRepository
    {
        Task<Payment> AddAsync(Payment payment);
        Task<Payment> Find(int id);
        Task UpdateAsync(Payment payment);
    }
}
