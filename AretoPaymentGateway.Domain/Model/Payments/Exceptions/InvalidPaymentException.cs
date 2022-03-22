using System;

namespace AretoPaymentGateway.Domain.Model.Payments.Exceptions
{
    public class InvalidPaymentException : Exception
    {
        public InvalidPaymentException() : base("The payment is invalid")
        {
        }
    }
}
