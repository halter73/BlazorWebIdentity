using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWeb.Client;

public class ClientAuthenticationStateProvider(PersistentComponentState persistentState) : AuthenticationStateProvider
{
    public const string PersistenceKey = $"__BlazorWeb__{nameof(AuthenticationState)}";

    private static readonly Task<AuthenticationState> _unauthenticatedTask =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // REVIEW: Is TryTakeFromJson correctly annotated? The "?" in "userInfo?.Email" should not be necessary.
        if (!persistentState.TryTakeFromJson<UserInfo>(PersistenceKey, out var userInfo)
            || userInfo?.Email is null || userInfo.UserId is null)
        {
            return _unauthenticatedTask;
        }

        Claim[] claims = [
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
            new Claim(ClaimTypes.Name, userInfo.Email),
            new Claim(ClaimTypes.Email, userInfo.Email) ];

        return Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims))));

        //return Task.FromResult(
        //    new AuthenticationState(new ClaimsPrincipal(
        //        new ClaimsIdentity(claims, "Identity.Application", userInfo.Email))));
    }
}

