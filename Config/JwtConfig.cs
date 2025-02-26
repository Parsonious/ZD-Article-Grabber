namespace ZD_Article_Grabber.Config
{
    public readonly record struct JwtConfig
    {
        public required string TokenKey { get; init; }
        public required string Issuer { get; init; }
        public required double ExpirationInMinutes { get; init; }
        public required string ApiKey { get; init; }

    }
}