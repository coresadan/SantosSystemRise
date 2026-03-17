namespace SantosSystemRise.Services;

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class SimpleAuthProvider : AuthenticationStateProvider
{
    private bool _isAuthenticated = false;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = _isAuthenticated
            ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Admin") }, "custom")
            : new ClaimsIdentity();

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void Login()
    {
        _isAuthenticated = true;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void Logout()
    {
        _isAuthenticated = false;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
