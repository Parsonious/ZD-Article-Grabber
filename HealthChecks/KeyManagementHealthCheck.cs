using Microsoft.Extensions.Diagnostics.HealthChecks;
using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.HealthChecks
{
    public class KeyManagementHealthCheck : IHealthCheck
    {
        private readonly IKeyHistoryService _keyHistoryService;
        private readonly IConfigOptions _config;
        private readonly string _keyFolder;

        public KeyManagementHealthCheck(IKeyHistoryService keyHistory, IConfigOptions config)
        {
            _keyHistoryService = keyHistory;
            _config = config;
            _keyFolder = _config.KeyManagement.KeyActiveFolder;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentKeyPath = Path.Combine(_keyFolder, "current.priv.pem");
                if (!File.Exists(currentKeyPath))
                {
                    return HealthCheckResult.Unhealthy("Current key file not found");
                }

                return HealthCheckResult.Healthy("Key management system is operational");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Key management system error", ex);
            }
        }
    }
}