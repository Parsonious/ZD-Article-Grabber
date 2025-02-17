using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using ZD_Article_Grabber.Interfaces;



namespace ZD_Article_Grabber.Controllers
{
    [Route("a/gt")]
    [ApiController]
    public class TokenController(IArticle article, IConfigOptions config) : ControllerBase
    {
        private readonly IArticle _article = article;
        private readonly IConfigOptions _config = config;

        [HttpGet("get-token")]
        public IActionResult GenerateToken(string title)
        {
            //validate api key before anything else is done
            if(!Request.Headers.TryGetValue("bak", out var apiKey) || apiKey != _config.Jwt.ApiKey )
            {
                return Unauthorized("Invalid Api Key");
            }
            //validate referer
            if ( !Request.Headers.TryGetValue("Referer", out var refererHeader) )
            {
                return BadRequest("Referer is required");
            }
            //safely parse out Referer Uri
            if ( !Uri.TryCreate(refererHeader, UriKind.Absolute, out var refererUri) )
            {
                return BadRequest("Invalid Referer");
            }
            //verify title
            if ( string.IsNullOrEmpty(title) )
            {
                return BadRequest("Title is required");
            }
            var sourceDomain = refererUri.Host;

            //if ( !_article.Exists(title) )
            //{
            //    return BadRequest("Title is not valid");
            //}

            //Generate Boilerplate JWT Claims
            var claims = new List<Claim>
            {
              new Claim("articleID", title), //temporary until a sql lite db can be set up to store article data
              new Claim("title", title),
              new Claim(JwtRegisteredClaimNames.Exp,
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

            //Generate JWT Token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Jwt.TokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config.Jwt.Issuer,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_config.Jwt.ExpirationInMinutes),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }
}
