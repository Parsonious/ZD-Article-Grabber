using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Services;

namespace ZD_Article_Grabber.Resources.Cache
{
    public class TokenCache(
        IMemoryCache cache,
        IKeyHistoryService keyHistory,
        ILogger<TokenCache> logger,
        ECDsaPool ecdsaPool,
        IConfigOptions config)
    {
        private readonly IMemoryCache _cache = cache;
        private readonly IKeyHistoryService _keyHistory = keyHistory;
        private readonly ILogger<TokenCache> _logger = logger;
        private readonly ECDsaPool _ecdsaPool = ecdsaPool;
        private readonly IConfigOptions _config = config;
        private static readonly TimeSpan _defaultCacheTime = TimeSpan.FromMinutes(10);
        private const string CURRENT_SIGNING_KEY_CACHE_KEY = "current_signing_key";

        public async Task<ECDsaSecurityKey> GetCurrentSigningKeyAsync()
        {
            return await _cache.GetOrCreateAsync(CURRENT_SIGNING_KEY_CACHE_KEY, async entry =>
            {
                entry.SlidingExpiration = _defaultCacheTime;
                entry.RegisterPostEvictionCallback(OnKeyEvicted);
                entry.SetPriority(CacheItemPriority.High);
                entry.Size = 2048; //default to 2kb estimate if it overflows bump it.

                var key = await LoadCurrentKey();
                _keyHistory.TrackKeyUsage(key.KeyId);
                return key;
            });
        }
        private void OnKeyEvicted(object key, object value, EvictionReason reason, object state)
        {
            if ( value is IDisposable disposable )
            {
                disposable.Dispose();
            }
            _logger.LogInformation($"Key {key} evicted from cache");
        }
        private async Task<ECDsaSecurityKey> LoadCurrentKey()
        {
            try
            {
                // Get ECDsa instance from pool
                var ecdsa = await _ecdsaPool.RentAsync();

                // Load current private key
                var keyPath = Path.Combine(_config.KeyManagement.KeyActiveFolder, "current.priv.pem");
                var keyText = await File.ReadAllTextAsync(keyPath);

                // Import key
                ecdsa.ImportFromPem(keyText);

                // Generate unique key ID based on file metadata
                var keyMetadata = File.GetCreationTimeUtc(keyPath);
                var keyId = $"ec-key-{keyMetadata:yyyyMMddHHmmss}";

                // Create and return security key
                return new ECDsaSecurityKey(ecdsa) { KeyId = keyId };
            }
            catch ( Exception ex )
            {
                _logger.LogError(ex, "Failed to load current signing key");
                throw;
            }
        }
    }
}
