using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWeb;

public class JsonAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string PersistenceKey = $"__internal__{nameof(AuthenticationState)}";

    private readonly PersistingComponentStateSubscription _subscription;
    private readonly AuthenticationState? _authenticationState;

    public JsonAuthenticationStateProvider(PersistentComponentState state)
    {
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        throw new NotImplementedException();
    }
}
