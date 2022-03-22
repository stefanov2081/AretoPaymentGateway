using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AretoPaymentGateway.Domain.Model.Payments;
using AretoPaymentGateway.Infrastructure.Persistence.EfCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AretoPaymentGateway.Interfaces.WebApi.Tests.Integration
{
    [TestClass]
    public class PaymentControllerTest
    {
        private readonly TestServer server;
        private readonly HttpClient client;
        private readonly ApplicationDbContext dbContext;
        private readonly string jsonWebToken;

        public PaymentControllerTest()
        {
            server = new TestServer(
                new WebHostBuilder()
                .UseEnvironment("Testing")
                    .UseConfiguration(
                        new ConfigurationBuilder()
                        .AddJsonFile("appsettings.Testing.json")
                        .AddEnvironmentVariables()
                        .Build())
                .UseStartup<Startup>());
            client = server.CreateClient();

            var configuration = server.Services.GetRequiredService<IConfiguration>();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            builder.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"))
                .UseInternalServiceProvider(serviceProvider);

            dbContext = new ApplicationDbContext(builder.Options);
            dbContext.Database.Migrate();

            jsonWebToken = Login().Result;
        }

        [TestMethod]
        public async Task Authorizing_a_payment_saves_the_payment_in_the_database_and_authorizes_it_with_Adyen()
        {
            var content = new JObject(
                new JProperty("amount", "10"),
                new JProperty("currency", "EUR"),
                new JProperty("reference", "Test authorization"),
                new JProperty("type", "scheme"),
                new JProperty("cardNumber", "test_4111111111111111"),
                new JProperty("expiryMonth", "test_03"),
                new JProperty("expiryYear", "test_2030"),
                new JProperty("securityCode", "test_737"));

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Payment/authorize");
            httpRequestMessage.Content = new StringContent(content.ToString(), System.Text.Encoding.UTF8, "application/json");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jsonWebToken);

            var httpResponse = await client.SendAsync(httpRequestMessage);
            var paymentId = int.Parse(await httpResponse.Content.ReadAsStringAsync());

            var lastPayment = dbContext.Payments
                .Include(payment => payment.Card)
                .OrderBy(x => x.Id)
                .Last();

            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.AreEqual(lastPayment.Id, paymentId);
            Assert.AreEqual(10, lastPayment.Amount);
            Assert.AreEqual("EUR", lastPayment.Currency);
            Assert.AreEqual("Test authorization", lastPayment.Reference);
            Assert.AreEqual("scheme", lastPayment.Card.Type);
            Assert.AreEqual("test_4111111111111111", lastPayment.Card.Number);
            Assert.AreEqual("test_03", lastPayment.Card.ExpiryMonth);
            Assert.AreEqual("test_2030", lastPayment.Card.ExpiryYear);
            Assert.AreEqual("test_737", lastPayment.Card.SecurityCode);
            Assert.AreEqual("Adyen", lastPayment.Acquirer);
            Assert.AreEqual(PaymentStatus.Authorized.ToString(), lastPayment.Status);
            Assert.IsNotNull(lastPayment.AcquirerReference); // Acquirer reference is generated by Adyen and is unknown ahead of time

            dbContext.Payments.Remove(lastPayment);
            await dbContext.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Capturing_a_payment_changes_its_status_to_settled()
        {
            var authorizeRequestContent = new JObject(
                new JProperty("amount", "15"),
                new JProperty("currency", "EUR"),
                new JProperty("reference", "Test capturing"),
                new JProperty("type", "scheme"),
                new JProperty("cardNumber", "test_4111111111111111"),
                new JProperty("expiryMonth", "test_03"),
                new JProperty("expiryYear", "test_2030"),
                new JProperty("securityCode", "test_737"));

            var authorizeHttpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Payment/authorize");
            authorizeHttpRequestMessage.Content = new StringContent(authorizeRequestContent.ToString(), System.Text.Encoding.UTF8, "application/json");
            authorizeHttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jsonWebToken);

            var authorizeHttpResponse = await client.SendAsync(authorizeHttpRequestMessage);
            var paymentId = int.Parse(await authorizeHttpResponse.Content.ReadAsStringAsync());

            var captureRequestContent = new JObject(
                new JProperty("paymentId", paymentId),
                new JProperty("amount", "15"),
                new JProperty("currency", "EUR"));

            var captureHttpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Payment/capture");
            captureHttpRequestMessage.Content = new StringContent(captureRequestContent.ToString(), System.Text.Encoding.UTF8, "application/json");
            captureHttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jsonWebToken);

            var captureHttpResponse = await client.SendAsync(captureHttpRequestMessage);

            var lastPayment = dbContext.Payments
                .Include(payment => payment.Card)
                .OrderBy(x => x.Id)
                .Last();

            Assert.AreEqual(HttpStatusCode.OK, captureHttpResponse.StatusCode);
            Assert.AreEqual(lastPayment.Id, paymentId);
            Assert.AreEqual(15, lastPayment.Amount);
            Assert.AreEqual("EUR", lastPayment.Currency);
            Assert.AreEqual("Test capturing", lastPayment.Reference);
            Assert.AreEqual("scheme", lastPayment.Card.Type);
            Assert.AreEqual("test_4111111111111111", lastPayment.Card.Number);
            Assert.AreEqual("test_03", lastPayment.Card.ExpiryMonth);
            Assert.AreEqual("test_2030", lastPayment.Card.ExpiryYear);
            Assert.AreEqual("test_737", lastPayment.Card.SecurityCode);
            Assert.AreEqual("Adyen", lastPayment.Acquirer);
            Assert.AreEqual(PaymentStatus.Settled.ToString(), lastPayment.Status);
            Assert.IsNotNull(lastPayment.AcquirerReference); // Acquirer reference is generated by Adyen and is unknown ahead of time

            Assert.AreEqual(lastPayment.Id, paymentId);

            dbContext.Payments.Remove(lastPayment);
            await dbContext.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Canceling_a_payment_changes_its_status_to_canceled()
        {
            var authorizeRequestContent = new JObject(
                new JProperty("amount", "20"),
                new JProperty("currency", "EUR"),
                new JProperty("reference", "Test cancel"),
                new JProperty("type", "scheme"),
                new JProperty("cardNumber", "test_4111111111111111"),
                new JProperty("expiryMonth", "test_03"),
                new JProperty("expiryYear", "test_2030"),
                new JProperty("securityCode", "test_737"));

            var authorizeHttpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Payment/authorize");
            authorizeHttpRequestMessage.Content = new StringContent(authorizeRequestContent.ToString(), System.Text.Encoding.UTF8, "application/json");
            authorizeHttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jsonWebToken);

            var authorizeHttpResponse = await client.SendAsync(authorizeHttpRequestMessage);
            var paymentId = int.Parse(await authorizeHttpResponse.Content.ReadAsStringAsync());

            var captureHttpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Payment/cancel/" + paymentId);
            captureHttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jsonWebToken);

            var captureHttpResponse = await client.SendAsync(captureHttpRequestMessage);

            var lastPayment = dbContext.Payments
                .Include(payment => payment.Card)
                .OrderBy(x => x.Id)
                .Last();

            Assert.AreEqual(HttpStatusCode.OK, captureHttpResponse.StatusCode);
            Assert.AreEqual(lastPayment.Id, paymentId);
            Assert.AreEqual(20, lastPayment.Amount);
            Assert.AreEqual("EUR", lastPayment.Currency);
            Assert.AreEqual("Test cancel", lastPayment.Reference);
            Assert.AreEqual("scheme", lastPayment.Card.Type);
            Assert.AreEqual("test_4111111111111111", lastPayment.Card.Number);
            Assert.AreEqual("test_03", lastPayment.Card.ExpiryMonth);
            Assert.AreEqual("test_2030", lastPayment.Card.ExpiryYear);
            Assert.AreEqual("test_737", lastPayment.Card.SecurityCode);
            Assert.AreEqual("Adyen", lastPayment.Acquirer);
            Assert.AreEqual(PaymentStatus.Canceled.ToString(), lastPayment.Status);
            Assert.IsNotNull(lastPayment.AcquirerReference); // Acquirer reference is generated by Adyen and is unknown ahead of time

            Assert.AreEqual(lastPayment.Id, paymentId);

            dbContext.Payments.Remove(lastPayment);
            await dbContext.SaveChangesAsync();
        }

        private async Task<string> Login()
        {
            var content = new JObject(
                new JProperty("username", "test@test.com"),
                new JProperty("password", "qwerty"));

            var httpContent = new StringContent(content.ToString(), System.Text.Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync("/api/Authentication/login", httpContent);

            httpResponse.EnsureSuccessStatusCode();

            var httpResponseContent = (JObject)JsonConvert.DeserializeObject(await httpResponse.Content.ReadAsStringAsync());
            var token = httpResponseContent.SelectToken("token").Value<string>();

            return token;
        }
    }
}