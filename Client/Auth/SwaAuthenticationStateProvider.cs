using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BlazorApp.Client.Auth
{
    public class SwaAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IConfiguration? _configuration;
        private readonly HttpClient? _httpClient;

        public SwaAuthenticationStateProvider(IConfiguration configuration, IWebAssemblyHostEnvironment environment)
        {
            _configuration = configuration;
            _httpClient = new HttpClient { BaseAddress = new Uri(environment.BaseAddress) };
        }
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var authDataUrl = _configuration.GetValue<string>("Authentication:Swa:AuthenticationDataUrl", "/.auth/me");
                var data = await _httpClient?.GetFromJsonAsync<AuthenticationModel>(authDataUrl);

                var principal = data?.ClientPrincipal;

                if (principal is null) return new AuthenticationState(new ClaimsPrincipal());
                
                principal.UserRoles = principal?.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);
                
                if (!principal.UserRoles.Any())  return new AuthenticationState(new ClaimsPrincipal());

                var identity = new ClaimsIdentity(principal.IdentityProvider);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
                identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
                identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }
        }
    }
}
