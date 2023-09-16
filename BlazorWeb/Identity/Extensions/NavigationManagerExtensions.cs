namespace Microsoft.AspNetCore.Components;

internal static class NavigationManagerExtensions
{
    public static void ReloadWithMessage(this NavigationManager navigationManager, string message)
    {
        var uri = GetUriWithMessageQueryParameter(navigationManager, newRelativeUri: null, message);
        navigationManager.NavigateTo(uri, forceLoad: true);
    }

    public static void NavigateToWithMessage(this NavigationManager navigationManager, string? relativeUri, string? message)
    {
        var uri = GetUriWithMessageQueryParameter(navigationManager, relativeUri, message);
        navigationManager.NavigateTo(uri);
    }

    private static string GetUriWithMessageQueryParameter(NavigationManager navigationManager, string? newRelativeUri, string? message)
    {
        var uriBuilder = new UriBuilder(navigationManager.Uri);

        if (newRelativeUri is not null)
        {
            uriBuilder.Path = newRelativeUri;
        }

        if (message is not null)
        {
            uriBuilder.Query = $"Message={message}";
        }

        return uriBuilder.ToString();
    }
}
