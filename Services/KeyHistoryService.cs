using ZD_Article_Grabber.Records;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ZD_Article_Grabber.Interfaces;
namespace ZD_Article_Grabber.Services
{
    public class KeyHistoryService : IKeyHistoryService, IDisposable
    {
        private readonly ILogger<KeyHistoryService> _logger;
        private readonly IConfigOptions _config;
        private readonly ConcurrentDictionary<string, KeyMetadata> _keyHistory = new();
        private readonly Timer _cleanupTimer;
        private readonly SemaphoreSlim _fileAccessSemaphore = new(1, 1);
        public KeyHistoryService(ILogger<KeyHistoryService> logger, IConfigOptions options)
        {
            _logger = logger;
            _config = options ?? throw new ArgumentNullException(nameof(options));
            LoadKeyHistoryAsync().GetAwaiter().GetResult(); // Initialize synchronously
            _cleanupTimer = new Timer(CleanupExpiredKeys, null,
                TimeSpan.FromMinutes(5), // Initial delay
                TimeSpan.FromHours(1));  // Regular interval
        }
        public ValueTask<bool> IsKeyValidAsync(string keyId)
        {
            if (_keyHistory.TryGetValue(keyId, out var metadata))
            {
                var isValid = metadata.ExpiresAt > DateTime.UtcNow;
                if (!isValid)
                {
                    _logger.LogWarning("Key {KeyId} has expired.", keyId);
                }
                return new ValueTask<bool>(isValid);
            }

            _logger.LogWarning("Key {KeyId} not found in history.", keyId);
            return new ValueTask<bool>(false);
        }
        public void TrackKeyUsage(string keyId)
        {
            if (_keyHistory.TryGetValue(keyId, out var metadata))
            {
                metadata.IncrementUsageCount();
                _logger.LogInformation("Key {KeyId} used. Total usage: {Count}", keyId, metadata.UsageCount);
            }
        }
        private async Task LoadKeyHistoryAsync()
        {
            try
            {
                if (!await _fileAccessSemaphore.WaitAsync(TimeSpan.FromSeconds(30)))
                {
                    throw new TimeoutException("Could not acquire file access lock");
                }

                var keyFiles = await Task.Run(() =>
                    Directory.GetFiles(_config.KeyManagement.KeyActiveFolder, "*.priv.pem"));

                foreach (var keyFile in keyFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(keyFile);
                        string keyId = Path.GetFileNameWithoutExtension(keyFile).Split('.')[0];
                        DateTime createdAt = fileInfo.CreationTimeUtc;
                        DateTime expiresAt = createdAt.AddDays(_config.KeyManagement.KeyLifetimeDays);

                        _keyHistory.AddOrUpdate(keyId,
                            new KeyMetadata(keyId, createdAt, expiresAt),
                            (_, existing) => new KeyMetadata(keyId, createdAt, expiresAt));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing key file: {KeyFile}", keyFile);
                    }
                }
            }
            finally
            {
                _fileAccessSemaphore.Release();
            }
        }
        private void CleanupExpiredKeys(object? state)
        {
            var expiredKeys = _keyHistory
                .Where(kvp => kvp.Value.ExpiresAt < DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach ( var keyId in expiredKeys )
            {
                if ( _keyHistory.TryRemove(keyId, out var metadata) )
                {
                    _logger.LogInformation("Removed expired key {KeyId} from history. " +
                        "Total usage count: {Count}", keyId, metadata.UsageCount);
                }
            }
        }
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _fileAccessSemaphore?.Dispose();
        }
    }
}