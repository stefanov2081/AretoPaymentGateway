using System.Net.Http;
using System.Threading.Tasks;
using AretoPaymentGateway.Domain.Model.Payments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AretoPaymentGateway.Infrastructure.PaymentAcquirers
{
    public class AdyenPaymentAcquirer : PaymentAcquirer
    {
        private const string MerchantAccount = "Brd67984ECOM";
        private const string ApiKey = "AQEjhmfuXNWTK0Qc+iSSgGBu8L3DHFuL2Xwote7CUul14k7Ltc0QwV1bDb7kfNy1WIxIIkxgBw==-jpfhcf75CthAXZK2jEPSU1vqR5CjM14msJ/xLNX98yI=-A2N+W)_R;,VyH6&^";

        private readonly HttpClient httpClient;

        public AdyenPaymentAcquirer(IHttpClientFactory httpClientFactory) : base("Adyen")
        {
            httpClient = httpClientFactory.CreateClient();
        }

        public override async Task<string> AuthorizePaymentAsync(Payment payment)
        {
            var content = new JObject(
                new JProperty("merchantAccount", MerchantAccount),
                new JProperty("reference", payment.Reference),
                new JProperty("amount", new JObject(
                    new JProperty("value", ConvertAmount(payment.Amount)),
                    new JProperty("currency", payment.Currency))),
                new JProperty("paymentMethod", new JObject(
                    new JProperty("type", payment.Card.Type),
                    new JProperty("encryptedCardNumber", payment.Card.Number),
                    new JProperty("encryptedExpiryMonth", payment.Card.ExpiryMonth),
                    new JProperty("encryptedExpiryYear", payment.Card.ExpiryYear),
                    new JProperty("encryptedSecurityCode", payment.Card.SecurityCode))));

            var httpContent = new StringContent(content.ToString(), System.Text.Encoding.UTF8, "application/json");
            httpContent.Headers.Add("x-api-key", ApiKey);

            var httpResponse = await httpClient.PostAsync("https://checkout-test.adyen.com/v68/payments", httpContent);

            httpResponse.EnsureSuccessStatusCode();

            var httpResponseContent = (JObject)JsonConvert.DeserializeObject(await httpResponse.Content.ReadAsStringAsync());
            var pspReference = httpResponseContent.SelectToken("pspReference").Value<string>();

            return pspReference;
        }

        public override async Task CapturePaymentAsync(Payment payment)
        {
            var content = new JObject(
                new JProperty("merchantAccount", MerchantAccount),
                new JProperty("reference", payment.Reference),
                new JProperty("amount", new JObject(
                    new JProperty("value", ConvertAmount(payment.Amount)),
                    new JProperty("currency", payment.Currency))));

            var httpContent = new StringContent(content.ToString(), System.Text.Encoding.UTF8, "application/json");
            httpContent.Headers.Add("x-api-key", ApiKey);

            var httpResponse = await httpClient.PostAsync("https://checkout-test.adyen.com/v68/payments/" + payment.AcquirerReference + "/captures", httpContent);

            httpResponse.EnsureSuccessStatusCode();
        }

        public override async Task CancelPaymentAsync(Payment payment)
        {
            var content = new JObject(
                new JProperty("merchantAccount", MerchantAccount),
                new JProperty("reference", payment.Reference));

            var httpContent = new StringContent(content.ToString(), System.Text.Encoding.UTF8, "application/json");
            httpContent.Headers.Add("x-api-key", ApiKey);

            var httpResponse = await httpClient.PostAsync("https://checkout-test.adyen.com/v68/payments/" + payment.AcquirerReference + "/cancels", httpContent);

            httpResponse.EnsureSuccessStatusCode();
        }

        private static int ConvertAmount(decimal amount)
        {
            return (int)(amount * 100);
        }
    }
}
