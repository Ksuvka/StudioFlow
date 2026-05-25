using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace StudioFlow.Services
{
    public class SimpleAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;

        public SimpleAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userId = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userId");

                if (!string.IsNullOrEmpty(userId))
                {
                    var userEmail = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userEmail");
                    var userName = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userName");
                    var userRole = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userRole");

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Email, userEmail ?? ""),
                        new Claim(ClaimTypes.Name, userName ?? ""),
                        new Claim(ClaimTypes.Role, userRole ?? "Client")
                    };

                    var identity = new ClaimsIdentity(claims, "SimpleAuth");
                    var user = new ClaimsPrincipal(identity);
                    return new AuthenticationState(user);
                }
            }
            catch { }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void NotifyUserLoggedIn()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLoggedOut()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}