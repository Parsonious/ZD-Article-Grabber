using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using ZD_Article_Grabber.Interfaces;
namespace ZD_Article_Grabber.Services
{
    public class KeyRotationService : BackgroundService
    {
        private readonly ILogger<KeyRotationService> _logger;
        private readonly IConfigOptions _config;
        private readonly string _keyFolder;
        private readonly TimeSpan _rotationInterval;
        private readonly TimeSpan _keyLifetime;
        private readonly SemaphoreSlim _rotationLock = new(1, 1);

        public KeyRotationService( ILogger<KeyRotationService> logger, IConfigOptions config)
        {
            _logger = logger;
            _config = config;
            _keyFolder = _config.KeyManagement.KeyFolder;
            _rotationInterval = TimeSpan.FromDays(_config.KeyManagement.RotationIntervalDays);
            _keyLifetime = TimeSpan.FromDays(_config.KeyManagement.KeyLifetimeDays);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RotateKeysIfNeeded();
                    await CleanupExpiredKeys();
                    await Task.Delay(_rotationInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during key rotation");
                }
            }
        }

         private async Task RotateKeysIfNeeded()
    {
        try
        {
            if (!await _rotationLock.WaitAsync(TimeSpan.FromSeconds(30)))
            {
                _logger.LogWarning("Could not acquire rotation lock - another rotation may be in progress");
                return;
            }

            var currentKeyPath = Path.Combine(_keyFolder, "current.priv.pem");
            if (!File.Exists(currentKeyPath) || await IsKeyExpiringAsync(currentKeyPath))
            {
                await GenerateNewKeyPair();
            }
        }
        finally
        {
            _rotationLock.Release();
        }
    }

       private async Task<bool> IsKeyExpiringAsync(string keyPath)
    {
        try
        {
            var fileInfo = new FileInfo(keyPath);
            var keyAge = DateTime.UtcNow - (await Task.Run(() => fileInfo.CreationTimeUtc));
            return keyAge > (_keyLifetime - TimeSpan.FromDays(7));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking key expiration for {KeyPath}", keyPath);
            return true; // Err on the side of caution
        }
    }
        private async Task GenerateNewKeyPair()
        {
            if(!Directory.Exists(_keyFolder))
            {
                Directory.CreateDirectory(_keyFolder);
                _logger.LogInformation("Created key folder: {KeyFolder}", _keyFolder);
            }
            using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var keyId = $"ec-key-{timestamp}";

            // Export keys
            var privateKey = ecdsa.ExportECPrivateKeyPem();
            var publicKey = ecdsa.ExportSubjectPublicKeyInfoPem();

            // Save new keys
            await File.WriteAllTextAsync(Path.Combine(_keyFolder, $"{keyId}.priv.pem"), privateKey);
            await File.WriteAllTextAsync(Path.Combine(_keyFolder, $"{keyId}.pub.pem"), publicKey);

            // Update current key symlinks
            await UpdateCurrentKeyPointersAsync(keyId);

            _logger.LogInformation("Generated new key pair with ID: {KeyId}", keyId);
        }
        private async Task CleanupExpiredKeys()
        {
            var retentionPeriod = TimeSpan.FromDays(
                double.Parse(_config.KeyManagement.RotationIntervalDays.ToString() ?? "180")); //default to 180 days if not set or read
            var files = Directory.GetFiles(_keyFolder, "*.priv.pem")
                .Where(f => !f.Contains("current"))
                .Where(f => (DateTime.UtcNow - File.GetCreationTime(f)) > retentionPeriod);

            foreach (var file in files)
            {
                try
                {
                    await Task.Run(() => File.Delete(file));
                    var pubKey = Path.ChangeExtension(file, ".pub.pem");
                    if (File.Exists(pubKey))
                    {
                        await Task.Run(() => File.Delete(pubKey));
                    }
                    _logger.LogInformation("Deleted expired key: {KeyPath}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting expired key: {KeyPath}", file);
                }
            }
        }

        private async Task UpdateCurrentKeyPointersAsync(string keyId)
    {
        var currentPrivPath = Path.Combine(_keyFolder, "current.priv.pem");
        var currentPubPath = Path.Combine(_keyFolder, "current.pub.pem");
        var newPrivPath = Path.Combine(_keyFolder, $"{keyId}.priv.pem");
        var newPubPath = Path.Combine(_keyFolder, $"{keyId}.pub.pem");

        try
        {
            // Use atomic operations for file updates
            var tempPrivPath = Path.Combine(_keyFolder, $"current.priv.{Guid.NewGuid()}.tmp");
            var tempPubPath = Path.Combine(_keyFolder, $"current.pub.{Guid.NewGuid()}.tmp");

            await Task.Run(() => File.Copy(newPrivPath, tempPrivPath));
            await Task.Run(() => File.Copy(newPubPath, tempPubPath));

            if (File.Exists(currentPrivPath)) await Task.Run(() => File.Delete(currentPrivPath));
            if (File.Exists(currentPubPath)) await Task.Run(() => File.Delete(currentPubPath));

            await Task.Run(() => File.Move(tempPrivPath, currentPrivPath));
            await Task.Run(() => File.Move(tempPubPath, currentPubPath));

            _logger.LogInformation("Updated current key pointers to: {KeyId}", keyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update key pointers for: {KeyId}", keyId);
            throw;
        }
    }
public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _rotationLock.Dispose();
    }
private bool ValidateKeyPair(ECDsa ecdsa)
{
    try
    {
        var testData = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(testData);
        }
        var signature = ecdsa.SignData(testData, HashAlgorithmName.SHA256);
        return ecdsa.VerifyData(testData, signature, HashAlgorithmName.SHA256);
    }
    catch
    {
        return false;
    }
}
    }
}