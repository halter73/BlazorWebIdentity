using BlazorWeb.Identity.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace BlazorWeb;

internal sealed class UserAccessor(
    IHttpContextAccessor httpContextAccessor,
    UserManager<BlazorWebUser> userManager,
    NavigationManager navigationManager)
{
    public async Task<BlazorWebUser> GetRequiredUserAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User ??
            throw new InvalidOperationException($"{nameof(GetRequiredUserAsync)} requires access to an {nameof(HttpContext)}.");
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            // Throws NavigationException, which is handled by the framework as a redirect.
            navigationManager.RedirectTo("/Identity/Account/InvalidUser", new()
            {
                ["Message"] = $"Error: Unable to load user with ID '{userManager.GetUserId(principal)}'.",
            });
        }

        return user;
    }
}
