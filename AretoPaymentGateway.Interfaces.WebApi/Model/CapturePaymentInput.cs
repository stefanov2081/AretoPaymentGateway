using System.ComponentModel.DataAnnotations;

namespace AretoPaymentGateway.Interfaces.WebApi.Model
{
    public class CapturePaymentInput
    {
        public int PaymentId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }
    }
}
