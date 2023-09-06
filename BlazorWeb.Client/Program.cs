using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ClientPersistentStateAuthenticationStateProvider>();

await builder.Build().RunAsync();

class ClientPersistentStateAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        //var user = new ClaimsPrincipal(new ClaimsIdentity());
        var user = new ClaimsPrincipal(new ClaimsIdentity("Identity.Application", "test@example.com", roleType: null));
        return Task.FromResult(new AuthenticationState(user));
    }
}
