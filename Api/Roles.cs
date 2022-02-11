using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorApp.Api
{
    public class Roles
    {
        readonly HttpClient _httpClient;
        readonly ILogger _logger;
        public Roles(ILogger<Roles> log, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = log;
        }

        [FunctionName(nameof(Roles))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string jwt = req.Headers["x-ms-client-principal"];
                if (jwt is null) return new OkObjectResult(new string[] { });

                byte[] bytes = WebEncoders.Base64UrlDecode(jwt);
                string json = System.Text.Encoding.UTF8.GetString(bytes);
                var roles = new List<string>();

                var principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var hasRoles = principal?.UserRoles?.Any() ?? false;
                if (!hasRoles) return new OkObjectResult(new string[] { });

                if (principal.UserDetails.Contains("gilroymenezes")) roles.Add("Administrator");
                foreach(var role in principal.UserRoles)
                {
                    roles.Add(role);
                }
                return new OkObjectResult(roles);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
    public class AuthenticationModel
    {
        public ClientPrincipal ClientPrincipal { get; set; }
    }
    public class ClientPrincipal
    {
        public string IdentityProvider { get; set; }
        public string UserId { get; set; }
        public string UserDetails { get; set; }
        public IEnumerable<string> UserRoles { get; set; }
    }
}
