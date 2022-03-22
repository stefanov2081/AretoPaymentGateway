using System;

namespace AretoPaymentGateway.Domain.Model.Payments
{
    public struct Card/* : IValueObject<Card>*/
    {
        public Card(string number, string expiryMonth, string expiryYear, string securityCode, string type)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentNullException(nameof(number));
            if (string.IsNullOrWhiteSpace(expiryMonth))
                throw new ArgumentNullException(nameof(expiryMonth));
            if (string.IsNullOrWhiteSpace(expiryYear))
                throw new ArgumentNullException(nameof(expiryYear));
            if (string.IsNullOrWhiteSpace(securityCode))
                throw new ArgumentNullException(nameof(securityCode));
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentNullException(nameof(type));


            Number = number;
            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
            SecurityCode = securityCode;
            Type = type;
        }

        public string Number { get; init; }
        public string ExpiryMonth { get; init; }
        public string ExpiryYear { get; init; }
        public string SecurityCode { get; init; }
        public string Type { get; init; }
    }
}
