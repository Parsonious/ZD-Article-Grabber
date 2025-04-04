namespace ZD_Article_Grabber.Records
{
public record class KeyMetadata
    {
        public string KeyId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime ExpiresAt { get; init; }
        private int _usageCount;
        public int UsageCount => _usageCount;

        public KeyMetadata(string keyId, DateTime createdAt, DateTime expiresAt)
        {
            KeyId = keyId;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
            _usageCount = 0;
        }
        public int IncrementUsageCount()
        {
            return Interlocked.Increment(ref _usageCount);
        }
    }
}