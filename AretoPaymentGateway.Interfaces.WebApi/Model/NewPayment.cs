using System.ComponentModel.DataAnnotations;
using AretoPaymentGateway.Application.Payments;

namespace AretoPaymentGateway.Interfaces.WebApi.Model
{
    public class NewPayment : INewPayment
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }

        [Required]
        public string Reference { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string CardNumber { get; set; }

        [Required]
        public string ExpiryMonth { get; set; }

        [Required]
        public string ExpiryYear { get; set; }

        [Required]
        public string SecurityCode { get; set; }
    }
}
