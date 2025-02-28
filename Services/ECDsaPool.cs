using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ZD_Article_Grabber.Services
{
    public sealed class ECDsaPool : IDisposable
    {
        private readonly ObjectPool<ECDsa> _pool;
        private readonly ConcurrentDictionary<string, byte[]> _keyCache = new();
        private readonly SemaphoreSlim _initializationLock = new(1, 1);
        private bool _isDisposed;

        public ECDsaPool(ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ECDsaPool>();
            var poolPolicy = new ECDsaPoolPolicy(logger);
            _pool = new DefaultObjectPool<ECDsa>(poolPolicy, Environment.ProcessorCount * 2);
        }

        public async ValueTask<ECDsa> RentAsync()
        {
            ThrowIfDisposed();
            return await Task.FromResult(_pool.Get());
        }

        public void Return(ECDsa ecdsa)
        {
            if ( ecdsa == null ) return;
            if ( !_isDisposed ) _pool.Return(ecdsa);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(ECDsaPool));
        }

        public void Dispose()
        {
            if ( _isDisposed ) return;
            _isDisposed = true;
            _initializationLock.Dispose();
        }

        private class ECDsaPoolPolicy(ILogger logger) : IPooledObjectPolicy<ECDsa>
        {
            private readonly ILogger _logger = logger;

            public ECDsa Create()
            {
                var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
                _logger.LogDebug("Created new ECDsa instance");
                return ecdsa;
            }

            public bool Return(ECDsa obj)
            {
                try
                {
                    // Validate the object is still usable
                    var testData = new byte[32];
                    using var rng = RandomNumberGenerator.Create();
                    rng.GetBytes(testData);

                    var signature = obj.SignData(testData, HashAlgorithmName.SHA256);
                    return obj.VerifyData(testData, signature, HashAlgorithmName.SHA256);
                }
                catch ( Exception ex )
                {
                    _logger.LogWarning(ex, "ECDsa instance validation failed, will create new");
                    return false;
                }
            }
        }
    }

}
