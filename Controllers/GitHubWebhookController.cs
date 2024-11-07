using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System;

namespace ZD_Article_Grabber.Controllers
{
    [ApiController]
    [Route("a/github/webhook")]
    public class GitHubWebhookController(IConfiguration configuration ) : ControllerBase
    {
        private readonly string _webhookSecret = configuration["GitHubWebhookSecret"] ?? throw new ArgumentNullException(nameof(configuration));
        private readonly string _repositoryPath = configuration["RepositoryPath"] ?? throw new ArgumentNullException(nameof(configuration));
     
        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            // Read the request body
            using StreamReader reader = new StreamReader(Request.Body);
            string? payload = await reader.ReadToEndAsync();

            //Get the GitHub Signature from the headers
            if ( !Request.Headers.TryGetValue("X-Hub-Signature-256", out var signatureHeaderValue))
            {
                return Unauthorized();
            }

            string signature = signatureHeaderValue.ToString();

            //Verify the payload using the secret

            var isValid = IsValidSignature(payload, signature, _webhookSecret);

            if ( !isValid )
            {
                return Unauthorized();
            }

            //Process the webhook payload
            var eventType = Request.Headers["X-GitHub-Event"];
            switch ( eventType )
            {
                case "push":
                    //Trigger git pull op
                    try
                    {
                        await UpdateRepositoryAsync();
                    }
                    catch ( Exception ) 
                    {
                        return StatusCode(500, "Failed to update repository");
                    }
                    break;
            }
            return Ok();
        }
        private bool IsValidSignature(string payload, string signatureWithPrefix, string secret)
        {
            var signaturePrefix = "sha256";
            if(!signatureWithPrefix.StartsWith(signaturePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var signature = signatureWithPrefix.Substring(signaturePrefix.Length);

            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(secretBytes);
            var hasBytes = hmac.ComputeHash(payloadBytes);

            var hasString = BitConverter.ToString(hasBytes).Replace("-","").ToLowerInvariant();

            return hasString.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }
        private async Task UpdateRepositoryAsync()
        {
            //Commands to execute
            string command = "git";
            string arguments = "pull";

            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = _repositoryPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = new System.Diagnostics.Process
            {
                StartInfo = processStartInfo,
            };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Git pull failed with exit code {process.ExitCode}: {error}");
            }
        }
    }
}

