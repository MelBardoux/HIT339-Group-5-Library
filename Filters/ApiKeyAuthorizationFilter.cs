using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;

namespace LibrarySystem.Filters
{
    public class ApiKeyAuthorizationFilter : IAuthorizationFilter
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthorizationFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Read the expected API key from User Secrets / configuration.
            var configuredKey = _configuration["ApiSettings:ApiKey"];

            // Fail closed if the API key has not been configured.
            if (string.IsNullOrEmpty(configuredKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Require exactly one X-API-Key header.
            if (!context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var providedValues) ||
                providedValues.Count != 1)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var providedKey = providedValues[0];

            // Hash both values so they have the same length before
            // performing the fixed-time comparison.
            var configuredHash = SHA256.HashData(
                Encoding.UTF8.GetBytes(configuredKey));

            var providedHash = SHA256.HashData(
                Encoding.UTF8.GetBytes(providedKey));

            if (!CryptographicOperations.FixedTimeEquals(
                    providedHash,
                    configuredHash))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}