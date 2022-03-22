using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AretoPaymentGateway.Infrastructure.Persistence.EfCore.Model
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CardId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string Reference { get; set; }

        public DateTime Date { get; set; }

        public string Acquirer { get; set; }

        public string AcquirerReference { get; set; }

        [Required]
        public string Status { get; set; }

        public ApplicationUser User { get; set; }
        public Card Card { get; set; }
    }
}
