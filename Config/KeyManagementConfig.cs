namespace ZD_Article_Grabber.Config
{
    public sealed class KeyManagementConfig
    {
        private const int DEFAULT_ROTATION_DAYS = 30;
        private const int DEFAULT_LIFETIME_DAYS = 90;
        private const int DEFAULT_RETENTION_DAYS = 180;
        private const int DEFAULT_MIN_AGE = 1;
        private const int DEFAULT_WARNING_DAYS = 7;

        public string KeyActiveFolder { get; init; } = "Keys";
        public string KeyArchiveFolder { get; init; } = "Keys/Archive";
        public int RotationIntervalDays { get; init; } = DEFAULT_ROTATION_DAYS;
        public int KeyLifetimeDays { get; init; } = DEFAULT_LIFETIME_DAYS;
        public int RetentionPeriodDays { get; init; } = DEFAULT_RETENTION_DAYS;
        public int MinimumKeyAge { get; init; } = DEFAULT_MIN_AGE;
        public int WarningThresholdDays { get; init; } = DEFAULT_WARNING_DAYS;
    }
}