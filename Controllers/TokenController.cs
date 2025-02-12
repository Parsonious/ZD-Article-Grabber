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
            if(!Request.Headers.TryGetValue("bak", out var apiKey) || apiKey != _config.Jwt.ApiKey )
            {
                return Unauthorized("Invalid Api Key");
            }
            //verify title
            if ( string.IsNullOrEmpty(title) )
            {
                return BadRequest("Title is required");
            }
            if ( !_article.Exists(title) )
            {
                return BadRequest("Title is not valid");
            }

            //Generate JWT Claims
            var claims = new[]
            {
              new Claim("articleID", title), //temporary until a sql lite db can be set up to store article data
              new Claim("title", title),
              new Claim(JwtRegisteredClaimNames.Exp,
                  new DateTimeOffset(DateTime.UtcNow.AddMinutes(_config.Jwt.ExpirationInMinutes)).ToUnixTimeSeconds().ToString())
            };

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
