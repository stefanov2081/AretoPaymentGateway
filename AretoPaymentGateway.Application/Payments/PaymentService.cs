using System;
using System.Threading.Tasks;
using AretoPaymentGateway.Domain.Model.Payments;
using AretoPaymentGateway.Domain.Model.Payments.Exceptions;

namespace AretoPaymentGateway.Application.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly PaymentAcquirer paymentAcquirer;

        public PaymentService(IPaymentRepository paymentRepository, PaymentAcquirer paymentAcquirer)
        {
            this.paymentRepository = paymentRepository;
            this.paymentAcquirer = paymentAcquirer;
        }

        public async Task<int> AuthorizePaymentAsync(INewPayment newPayment)
        {
            var payment = await paymentRepository.AddAsync(
                new Payment(
                    newPayment.Amount,
                    newPayment.Currency,
                    newPayment.Reference,
                    PaymentStatus.Received,
                    new Card(
                        newPayment.CardNumber,
                        newPayment.ExpiryMonth,
                        newPayment.ExpiryYear,
                        newPayment.SecurityCode,
                        newPayment.Type)));

            var paymentAcquirerReference = await paymentAcquirer.AuthorizePaymentAsync(payment);
            payment.Authorize(paymentAcquirer.Name, paymentAcquirerReference);
            await paymentRepository.UpdateAsync(payment);

            return payment.Id;
        }

        public async Task CapturePaymentAsync(int paymentId, decimal amount, string currency)
        {
            var payment = await paymentRepository.Find(paymentId);

            if (payment is null)
            {
                throw new InvalidPaymentException();
            }
            else if (payment.Status != PaymentStatus.Authorized)
            {
                throw new ArgumentException("Payment is in invalid state");
            }
            else if (payment.Currency != currency || payment.Amount < amount)
            {
                throw new ArgumentException("Payment has invalid currency or amount");
            }

            await paymentAcquirer.CapturePaymentAsync(payment);
            payment.Capture();
            await paymentRepository.UpdateAsync(payment);
        }

        public async Task CancelPaymentAsync(int paymentId)
        {
            var payment = await paymentRepository.Find(paymentId);

            if (payment is null)
            {
                throw new InvalidPaymentException();
            }
            else if (payment.Status != PaymentStatus.Authorized)
            {
                throw new ArgumentException("Payment is in invalid state");
            }

            await paymentAcquirer.CancelPaymentAsync(payment);
            payment.Cancel();
            await paymentRepository.UpdateAsync(payment);
        }
    }
}
