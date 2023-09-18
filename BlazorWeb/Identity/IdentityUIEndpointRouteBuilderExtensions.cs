using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using BlazorWeb.Components.Pages.Identity.Account;
using BlazorWeb.Identity.Data;
using Microsoft.Extensions.Primitives;
using BlazorWeb.Components.Pages.Identity.Account.Manage;
using Microsoft.AspNetCore.Authentication;

namespace BlazorWeb.Identity;

internal static class IdentityUIEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapIdentityUI(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("IdentityUI");

        var routeGroup = endpoints.MapGroup("/Identity");

        routeGroup.MapPost("/Account/Manage/DownloadPersonalData", async (
            HttpContext context,
            [FromServices] UserManager<BlazorWebUser> userManager,
            [FromServices] AuthenticationStateProvider authenticationStateProvider) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
            {
                return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
            }

            var userId = await userManager.GetUserIdAsync(user);
            logger.LogInformation("User with ID '{UserId}' asked for their personal data.", userId);

            // Only include personal data for download
            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(BlazorWebUser).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            var logins = await userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            personalData.Add($"Authenticator Key", (await userManager.GetAuthenticatorKeyAsync(user))!);
            var fileBytes = JsonSerializer.SerializeToUtf8Bytes(personalData);

            context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
            return Results.File(fileBytes, contentType: "application/json", fileDownloadName: "PersonalData.json");
        });

        routeGroup.MapPost("/Account/PerformExternalLogin", (
            HttpContext context,
            [FromServices] SignInManager<BlazorWebUser> signInManager,
            [FromForm] string provider,
            [FromForm] string returnUrl) =>
        {
            IEnumerable<KeyValuePair<string, StringValues>> query = [
                new("ReturnUrl", returnUrl),
                new("Action", ExternalLogin.LoginCallbackAction)];

            var redirectUrl = UriHelper.BuildRelative(
                context.Request.PathBase,
                $"/Identity/Account/ExternalLogin",
                QueryString.Create(query));
                
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Results.Challenge(properties, [provider]);
        });

        routeGroup.MapPost("/Account/Manage/LinkExternalLogin", async (
            HttpContext context,
            [FromServices] UserManager<BlazorWebUser> UserManager,
            [FromServices] SignInManager<BlazorWebUser> signInManager,
            [FromForm] string provider) =>
        {
            // Clear the existing external cookie to ensure a clean login process
            await context.SignOutAsync(IdentityConstants.ExternalScheme);

            var redirectUrl = UriHelper.BuildRelative(
                context.Request.PathBase,
                $"/Identity/Account/Manage/ExternalLogins",
                QueryString.Create("Action", ExternalLogins.LinkLoginCallbackAction));
                
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, UserManager.GetUserId(context.User));
            return Results.Challenge(properties, [provider]);
        });

        return routeGroup;
    }
}
