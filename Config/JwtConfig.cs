namespace ZD_Article_Grabber.Config
{
    public class JwtConfig
    {
        public required string TokenKey { get; set; }
        public required string Issuer { get; set; }
        public required double ExpirationInMinutes { get; set; }
        public required string ApiKey { get; set; }

    }
}
