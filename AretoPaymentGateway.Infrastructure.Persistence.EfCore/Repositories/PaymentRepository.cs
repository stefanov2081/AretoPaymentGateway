using System;
using System.Linq;
using System.Threading.Tasks;
using AretoPaymentGateway.Domain.Model.Payments;
using Microsoft.EntityFrameworkCore;
using DbModel = AretoPaymentGateway.Infrastructure.Persistence.EfCore.Model;

namespace AretoPaymentGateway.Infrastructure.Persistence.EfCore.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IUserContext userContext;

        public PaymentRepository(ApplicationDbContext dbContext, IUserContext userContext)
        {
            this.dbContext = dbContext;
            this.userContext = userContext;
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            var card = dbContext.Cards.FirstOrDefault(x =>
                x.UserId == userContext.UserId &&
                x.Number == payment.Card.Number &&
                x.ExpiryMonth == payment.Card.ExpiryMonth &&
                x.ExpiryYear == payment.Card.ExpiryYear);

            if (card is null)
            {
                card = new DbModel.Card()
                {
                    UserId = userContext.UserId,
                    Number = payment.Card.Number,
                    ExpiryMonth = payment.Card.ExpiryMonth,
                    ExpiryYear = payment.Card.ExpiryYear,
                    SecurityCode = payment.Card.SecurityCode,
                    Type = payment.Card.Type
                };
            }

            var dbPayment = dbContext.Payments.Add(
                new DbModel.Payment()
                {
                    UserId = userContext.UserId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Date = payment.Date,
                    Reference = payment.Reference,
                    Status = payment.Status.ToString(),
                    Card = card
                })
                .Entity;

            await dbContext.SaveChangesAsync();

            return ToIncompletePayment(dbPayment);
        }

        public async Task<Payment> Find(int id)
        {
            return ToCompletePayment(
                await dbContext.Payments
                    .Include(payment => payment.Card)
                    .FirstOrDefaultAsync(x => x.UserId == userContext.UserId && x.Id == id));
        }

        public async Task UpdateAsync(Payment payment)
        {
            var dbPayment = await dbContext.Payments.FindAsync(payment.Id);

            dbPayment.Acquirer = payment.Acquirer;
            dbPayment.AcquirerReference = payment.AcquirerReference;
            dbPayment.Status = payment.Status.ToString();

            await dbContext.SaveChangesAsync();
        }

        private static Payment ToIncompletePayment(DbModel.Payment dbPayment)
        {
            if (dbPayment is null)
            {
                return null;
            }

            return new Payment(
                dbPayment.Id,
                dbPayment.Amount,
                dbPayment.Currency,
                dbPayment.Reference,
                (PaymentStatus)Enum.Parse(typeof(PaymentStatus),
                dbPayment.Status),
                    new Card(
                        dbPayment.Card.Number,
                        dbPayment.Card.ExpiryMonth,
                        dbPayment.Card.ExpiryYear,
                        dbPayment.Card.SecurityCode,
                        dbPayment.Card.Type),
                dbPayment.Date);
        }

        private static Payment ToCompletePayment(DbModel.Payment dbPayment)
        {
            if (dbPayment is null)
            {
                return null;
            }

            return new Payment(
                dbPayment.Id,
                dbPayment.Amount,
                dbPayment.Currency,
                dbPayment.Reference,
                dbPayment.Acquirer,
                dbPayment.AcquirerReference,
                (PaymentStatus)Enum.Parse(typeof(PaymentStatus),
                dbPayment.Status),
                    new Card(
                        dbPayment.Card.Number,
                        dbPayment.Card.ExpiryMonth,
                        dbPayment.Card.ExpiryYear,
                        dbPayment.Card.SecurityCode,
                        dbPayment.Card.Type),
                dbPayment.Date);
        }
    }
}
