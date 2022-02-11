using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BlazorApp.Client.Auth
{
    public class SwaAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IConfiguration? _configuration;
        private readonly HttpClient _httpClient;

        public SwaAuthenticationStateProvider(IConfiguration configuration, IWebAssemblyHostEnvironment environment)
        {
            _configuration = configuration;
            _httpClient = new HttpClient { BaseAddress = new Uri(environment.BaseAddress) };
        }
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<AuthenticationModel>("/.auth/me");

                var principal = data?.ClientPrincipal ?? new ClientPrincipal();

                if (principal is null) return new AuthenticationState(new ClaimsPrincipal());

                principal.UserRoles = principal?.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

                var hasRoles = principal?.UserRoles?.Any() ?? false;
                if (!hasRoles) return new AuthenticationState(new ClaimsPrincipal());

                var identity = new ClaimsIdentity(principal?.IdentityProvider);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal?.UserId ?? String.Empty));
                identity.AddClaim(new Claim(ClaimTypes.Name, principal?.UserDetails ?? String.Empty));
                identity.AddClaims(principal?.UserRoles?.Select(r => new Claim(ClaimTypes.Role, r)) ?? new List<Claim>());
                identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));

                var roles = await _httpClient.GetFromJsonAsync<string[]>("/api/roles");

                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch 
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }
        }
    }
}
