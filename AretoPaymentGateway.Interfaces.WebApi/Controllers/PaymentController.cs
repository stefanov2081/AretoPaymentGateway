using System.Threading.Tasks;
using AretoPaymentGateway.Application.Payments;
using AretoPaymentGateway.Interfaces.WebApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AretoPaymentGateway.Interfaces.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
        }

        [HttpPost("authorize")]
        public async Task<IActionResult> AuthorizePaymentAsync([FromBody] NewPayment newPayment)
        {
            var paymentId = await paymentService.AuthorizePaymentAsync(newPayment);

            return Ok(paymentId);
        }

        [HttpPost("capture")]
        public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentInput capturePaymentInput)
        {
            await paymentService.CapturePaymentAsync(
                capturePaymentInput.PaymentId,
                capturePaymentInput.Amount,
                capturePaymentInput.Currency);

            return Ok();
        }

        [HttpPost("cancel/{paymentId}")]
        public async Task<IActionResult> CancelPayment(int paymentId)
        {
            await paymentService.CancelPaymentAsync(paymentId);

            return Ok();
        }
    }
}
