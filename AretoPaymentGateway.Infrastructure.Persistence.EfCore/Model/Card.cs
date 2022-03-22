using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AretoPaymentGateway.Infrastructure.Persistence.EfCore.Model
{
    [Index(nameof(Number), nameof(ExpiryMonth), nameof(ExpiryYear))]
    public class Card
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public string ExpiryMonth { get; set; }

        [Required]
        public string ExpiryYear { get; set; }

        [Required]
        public string SecurityCode { get; set; }

        [Required]
        public string Type { get; set; }

        public ApplicationUser User { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
