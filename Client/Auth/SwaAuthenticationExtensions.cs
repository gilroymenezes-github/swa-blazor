using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Client.Auth
{
    public static class SwaAuthenticationExtensions
    {
        public static IServiceCollection AddSwaAuthentication(this IServiceCollection services)
        {
            return services
                .AddAuthorizationCore()
                .AddScoped<AuthenticationStateProvider, SwaAuthenticationStateProvider>();
        }
    }
}
