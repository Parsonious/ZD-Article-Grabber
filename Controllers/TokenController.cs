using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using ZD_Article_Grabber.Interfaces;



namespace ZD_Article_Grabber.Controllers
{
    [Route("a/gt")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        //private readonly IArticle _article;
        public required IConfigOptions _config;
        public required IKeyHistoryService _keyHistory;
        public required ECDsaSecurityKey _ecdsaKey;

        private const string DEBUG_TOKEN_ENDPOINT = "get-token";
        private const string DEBUG_PUBLIC_KEY_ENDPOINT = "get-public-key";
        private const string RELEASE_PUBLIC_KEY_ENDPOINT = "8a8d2dbaeef843c20813c53687e8b20a"; //public key endpoint
        private const string RELEASE_TOKEN_ENDPOINT = "6765742d746f6b656e"; //hex encoded get-token
        private const string CURRENT_PRIVATE_KEY = "current.priv.pem";
        private const string CURRENT_PUBLIC_KEY = "current.pub.pem";

        public TokenController(IConfigOptions config, IKeyHistoryService keyHistory)
        {
            _config = config;
            _keyHistory = keyHistory;
            _ecdsaKey = LoadKey();
        }
        private protected ECDsaSecurityKey LoadKey()
        {
            // Get the full path for the private key
            string privKeyPath = Path.Combine(_config.KeyManagement.KeyActiveFolder, CURRENT_PRIVATE_KEY);

            // Read and create the key
            string? priv = System.IO.File.ReadAllText(privKeyPath);
            ECDsa? ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(priv);

            // Generate a unique key ID based on file metadata and hash
            DateTime keyMetadata = System.IO.File.GetCreationTime(privKeyPath);
            byte[] keyBytes = System.IO.File.ReadAllBytes(privKeyPath);
            string keyHash = Convert.ToHexString(SHA256.HashData(keyBytes)).Substring(0, 8);
            string keyId = $"ec-{keyHash}-{keyMetadata:yyyyMMdd}";

            return new ECDsaSecurityKey(ecdsa)
            {
                KeyId = keyId
            };
        }

        #if DEBUG
        [HttpGet(DEBUG_TOKEN_ENDPOINT)]
        #else
        [HttpGet(RELEASE_TOKEN_ENDPOINT)] //hex encoded get-token
        #endif

        public IActionResult GenerateToken(string title)
        {
            //validate api key before anything else is done
            if(!Request.Headers.TryGetValue("bak", out var apiKey) )
            {
                return Unauthorized("Call a locksmith");
            }
            //validate referer
            if ( !Request.Headers.TryGetValue("Referer", out var refererHeader) )
            {
                return BadRequest("Referer is required");
            }
            //safely parse out Referer Uri
            // Ensure referer has a scheme
            string refererString = refererHeader.ToString();
            if ( !refererString.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !refererString.StartsWith("https://", StringComparison.OrdinalIgnoreCase) )
            {
                refererString = $"https://{refererString}";
            }
            if ( !Uri.TryCreate(refererString, UriKind.Absolute, out var refererUri) )
            {
                return BadRequest($"Invalid Referer: {refererString}");
            }
            //verify title
            if ( string.IsNullOrEmpty(title) )
            {
                return BadRequest("Title is required");
            }
            var sourceDomain = refererUri.Host.ToLowerInvariant();

            //validate title
            if ( !_config.Referer.AllowedDomains.Contains(sourceDomain) )
            {
                return Unauthorized("Invalid referer domain");
            }

            //if ( !_article.Exists(title) )
            //{
            //    return BadRequest("Title is not valid");
            //}

            //Generate Boilerplate JWT Claims
            var claims = new List<Claim>
            {
              new ("articleID", title), //temporary until a sql lite db can be set up to store article data
              new ("title", title),
              new (JwtRegisteredClaimNames.Exp,
                  new DateTimeOffset(DateTime.UtcNow.AddMinutes(_config.Jwt.ExpirationInMinutes)).ToUnixTimeSeconds().ToString())
            };
            //Conditionally add domain specific claims
            if(_config.DomainClaims.Settings.TryGetValue(sourceDomain, out var domainSettings))
            {
                foreach ( var claimConfig in domainSettings.Claims )
                {
                    claims.Add(new Claim(claimConfig.Type, claimConfig.Value));
                }
            }

            _keyHistory.TrackKeyUsage(_ecdsaKey.KeyId);

            //Generate JWT Token
            JwtSecurityToken token = new(
                issuer: _config.Jwt.Issuer,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_config.Jwt.ExpirationInMinutes),
                signingCredentials: new SigningCredentials( _ecdsaKey, SecurityAlgorithms.EcdsaSha256, _ecdsaKey.KeyId )
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
        #if DEBUG
        [HttpGet(DEBUG_PUBLIC_KEY_ENDPOINT)] 
        #else
        [HttpGet(RELEASE_PUBLIC_KEY_ENDPOINT)] 
        #endif
        public IActionResult GetPublicKey()
        {
            string pubKeyPath = Path.Combine(_config.KeyManagement.KeyActiveFolder, CURRENT_PUBLIC_KEY);

            if( !System.IO.File.Exists(pubKeyPath) )
            {
                return NotFound("Public key not found");
            }

            string publicKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(System.IO.File.ReadAllText(pubKeyPath)));
            return Ok(new { publicKey });
        }
    }
}
