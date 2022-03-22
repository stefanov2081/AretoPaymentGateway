using System;

namespace AretoPaymentGateway.Domain.Model.Payments
{
    public class Payment/* : IEntity<Payment>*/
    {
        private readonly int id;
        private readonly decimal amount;
        private readonly string currency;
        private readonly string reference;
        private PaymentStatus status;
        private readonly Card card;
        private readonly DateTime date;
        private string acquirer;
        private string acquirerReference;

        // Transient constructor
        public Payment(decimal amount, string currency, string reference, PaymentStatus status, Card card)
        {
            this.amount = amount;
            this.currency = currency;
            this.reference = reference;
            this.status = status;
            this.card = card;

            date = DateTime.Now;
        }

        // Incomplete payment constructor
        public Payment(int id, decimal amount, string currency, string reference, PaymentStatus status, Card card, DateTime date)
        {
            this.id = id;
            this.amount = amount;
            this.currency = currency;
            this.reference = reference;
            this.status = status;
            this.card = card;
            this.date = date;
        }

        // Complete payment constructor
        public Payment(int id, decimal amount, string currency, string reference, string acquirer, string acquirerReference, PaymentStatus status, Card card, DateTime date)
        {
            this.id = id;
            Amount = amount;
            Currency = currency;
            this.reference = reference;
            this.status = status;
            this.acquirer = acquirer;
            this.acquirerReference = acquirerReference;
            this.card = card;
            this.date = date;
        }

        public int Id => id;
        public decimal Amount
        {
            get => amount;
            init
            {
                if (value < 0)
                {
                    throw new ArgumentException($"{nameof(Amount)} must be positive", nameof(Amount));
                }

                amount = value;
            }
        }

        public string Currency
        {
            get => currency;
            init
            {
                if (value.Length != 3)
                {
                    throw new ArgumentException($"{nameof(Currency)} must be three characters long", nameof(Currency));
                }

                currency = value;
            }
        }

        public string Reference => reference;
        public Card Card => card;
        public DateTime Date => date;
        public string Acquirer => acquirer;
        public string AcquirerReference => acquirerReference;
        public PaymentStatus Status => status;

        public void Authorize(string paymentAcquirer, string paymentAcquirerReference)
        {
            if (status != PaymentStatus.Received)
            {
                throw new InvalidOperationException("Payment must be in received state");
            }

            acquirer = paymentAcquirer;
            acquirerReference = paymentAcquirerReference;

            status = PaymentStatus.Authorized;
        }

        public void Capture()
        {
            if (status != PaymentStatus.Authorized)
            {
                throw new InvalidOperationException("Payment must be in authorized state");
            }

            status = PaymentStatus.Settled;
        }

        public void Cancel()
        {
            if (status != PaymentStatus.Authorized)
            {
                throw new InvalidOperationException("Payment must be in authorized state");
            }

            status = PaymentStatus.Canceled;
        }
    }
}
