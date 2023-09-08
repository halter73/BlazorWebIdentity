using System.Security.Claims;
using BlazorWeb.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BlazorWeb;

public class ServerAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly PersistingComponentStateSubscription _subscription;

    public ServerAuthenticationStateProvider(IHttpContextAccessor contextAccessor, PersistentComponentState state, IOptions<IdentityOptions> identityOptions)
    {
        _contextAccessor = contextAccessor;

        var userIdKey = identityOptions.Value.ClaimsIdentity.UserIdClaimType;
        var emailKey = identityOptions.Value.ClaimsIdentity.EmailClaimType;

        _subscription = state.RegisterOnPersisting(() =>
        {
            var user = RequiredHttpContext.User;

            state.PersistAsJson(ClientAuthenticationStateProvider.PersistenceKey, new UserInfo
            {
                UserId = user.FindFirst(userIdKey)?.Value,
                Email = user.FindFirst(emailKey)?.Value,
            });
            return Task.CompletedTask;
        });
    }

    private HttpContext RequiredHttpContext => 
        _contextAccessor.HttpContext ??
        throw new InvalidOperationException("IHttpContextAccessor HttpContext AsyncLocal missing!"); 

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(RequiredHttpContext.User));
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}
