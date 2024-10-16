using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionApp.Helper
{
    public class SqlHelper
    {
        private static AccessToken _cachedToken;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ILogger<SqlHelper> _logger;

        public SqlHelper(ILogger<SqlHelper> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetAzureSqlAccessTokenAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                // Check if the MSI_SECRET environment variable is set
                string msiSecret = Environment.GetEnvironmentVariable("MSI_SECRET");
                if (string.IsNullOrEmpty(msiSecret))
                {
                    // Return a fixed token if MSI_SECRET is not set
                    _logger.LogInformation("MSI_SECRET environment variable not found. Returning fixed token.");

                    string accessToken = "";
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        _logger.LogWarning("ACCESS_TOKEN environment variable not found. Cannot return a valid token.");
                        
                        throw new InvalidOperationException("No valid token available.");
                    }

                    _logger.LogInformation("Fixed token got.");

                    return accessToken;
                }

                // Check if the cached token is valid
                if (_cachedToken.Token == null || _cachedToken.ExpiresOn <= DateTime.UtcNow.AddMinutes(10))
                {
                    _logger.LogInformation("Refreshing Azure SQL access token");

                    string userAssignedClientId = Environment.GetEnvironmentVariable("UserAssignedClientId")
                        ?? throw new InvalidOperationException("UserAssignedClientId environment variable not set");

                    var credential = new DefaultAzureCredential(
                        new DefaultAzureCredentialOptions
                        {
                            ManagedIdentityClientId = userAssignedClientId,
                        }
                    );

                    _cachedToken = await credential.GetTokenAsync(
                        new TokenRequestContext(new[] { "https://database.windows.net/.default" })
                    );

                    _logger.LogInformation($"New token acquired.");
                }

                return _cachedToken.Token;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
