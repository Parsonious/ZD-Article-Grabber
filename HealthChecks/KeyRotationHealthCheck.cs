using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.HealthChecks
{
    public class KeyRotationHealthCheck : IHealthCheck
    {
        private readonly ILogger<KeyRotationHealthCheck> _logger;
        private readonly IConfigOptions _config;
        private readonly string _keyFolder;

        public KeyRotationHealthCheck(ILogger<KeyRotationHealthCheck> logger, IConfigOptions config)
        {
            _logger = logger;
            _config = config;
            _keyFolder = _config.KeyManagement.KeyActiveFolder;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                string currentPrivPath = Path.Combine(_keyFolder, "current.priv.pem");
                string currentPubPath = Path.Combine(_keyFolder, "current.pub.pem");

                if ( !File.Exists(currentPrivPath) || !File.Exists(currentPubPath) )
                {
                    return HealthCheckResult.Unhealthy("Current key files not found");
                }

                //Validate
                if ( !await ValidateKeyPairAsync(currentPrivPath, currentPubPath) )
                {
                    return HealthCheckResult.Unhealthy("Current key pair validation failed");
                }

                // Check key age
                var keyAge = DateTime.UtcNow - File.GetCreationTimeUtc(currentPrivPath);
                var warningThreshold = TimeSpan.FromDays(_config.KeyManagement.WarningThresholdDays);

                if ( keyAge > TimeSpan.FromDays(_config.KeyManagement.KeyLifetimeDays) )
                {
                    return HealthCheckResult.Unhealthy($"Current key has exceeded its lifetime of {_config.KeyManagement.KeyLifetimeDays} days");
                }

                if ( keyAge > warningThreshold )
                {
                    return HealthCheckResult.Degraded($"Current key is approaching its lifetime limit");
                }

                return HealthCheckResult.Healthy();
            }
            catch ( Exception ex )
            {
                _logger.LogError(ex, "Health check failed");
                return HealthCheckResult.Unhealthy("Health check failed", ex);
            }
        }

        private async Task<bool> ValidateKeyPairAsync(string privPath, string pubPath)
        {
            try
            {
                var privKeyText = await File.ReadAllTextAsync(privPath);
                var pubKeyText = await File.ReadAllTextAsync(pubPath);

                using var ecdsa = ECDsa.Create();
                ecdsa.ImportFromPem(privKeyText);

                // Create test signature
                var testData = new byte[32];
                using ( var rng = RandomNumberGenerator.Create() )
                {
                    rng.GetBytes(testData);
                }

                var signature = ecdsa.SignData(testData, HashAlgorithmName.SHA256);

                // Verify with public key
                using var pubKey = ECDsa.Create();
                pubKey.ImportFromPem(pubKeyText);

                return pubKey.VerifyData(testData, signature, HashAlgorithmName.SHA256);
            }
            catch ( Exception ex )
            {
                _logger.LogError(ex, "Key validation failed");
                return false;
            }
        }
    }
}
