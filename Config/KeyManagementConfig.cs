namespace ZD_Article_Grabber.Config
{
    public class KeyManagementConfig
    {
        public string KeyFolder { get; set; } = "Keys";
        public int RotationIntervalDays { get; set; } = 30;
        public int KeyLifetimeDays { get; set; } = 90;
        public int RetentionPeriodDays { get; set; } = 180;
        public int MinimumKeyAge { get; set; } = 1;
        public int WarningThresholdDays { get; set; } = 7;
    }
}