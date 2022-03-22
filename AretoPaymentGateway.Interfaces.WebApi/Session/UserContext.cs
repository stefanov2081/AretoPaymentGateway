using System.Security.Claims;
using AretoPaymentGateway.Infrastructure.Persistence.EfCore;
using Microsoft.AspNetCore.Http;

namespace AretoPaymentGateway.Interfaces.WebApi.Session
{
    public class UserContext : IUserContext
    {
        private readonly HttpContext httpContext;

        private int? userId;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public int UserId
        {
            get
            {
                if (userId.HasValue == false)
                {
                    userId = int.Parse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                return userId.Value;
            }
        }
    }
}
