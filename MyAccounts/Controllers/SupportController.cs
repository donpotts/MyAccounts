using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace MyAccounts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SupportController(IConfiguration config)
        {
            _config = config;
        }

        public class SupportRequest
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Message { get; set; }
        }

        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> Send([FromBody] SupportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("All fields are required.");

            var tenantId = _config["EmailSettings:TenantId"];
            var clientId = _config["EmailSettings:ClientId"];
            var clientSecret = _config["EmailSettings:ClientSecret"];
            var userId = _config["EmailSettings:UserId"];
            var supportEmail = _config["EmailSettings:SupportEmail"] ?? "don.potts@donpotts.com";

            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var options = new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };
            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var message = new Message
            {
                Subject = $"Support Request from {request.Name}",
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = $"From: {request.Name} <{request.Email}>\n\n{request.Message}"
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient { EmailAddress = new EmailAddress { Address = supportEmail } }
                }
            };

            var sendMailRequest = new SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = true
            };

            await graphClient.Users[userId].SendMail.PostAsync(sendMailRequest);
            return Ok();
        }
    }
}
