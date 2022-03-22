using System.ComponentModel.DataAnnotations;

namespace AretoPaymentGateway.Interfaces.WebApi.Areas.Identity.Account.Model
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
