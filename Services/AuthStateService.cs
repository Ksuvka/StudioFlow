using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace StudioFlow.Services
{
    public class AuthStateService : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly AppStateService _appStateService;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public AuthStateService(IJSRuntime jsRuntime, AppStateService appStateService)
        {
            _jsRuntime = jsRuntime;
            _appStateService = appStateService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userId = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userId");
                var userEmail = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userEmail");
                var userName = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userName");
                var userRole = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userRole");

                if (!string.IsNullOrEmpty(userId))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Email, userEmail ?? ""),
                        new Claim(ClaimTypes.Name, userName ?? ""),
                        new Claim(ClaimTypes.Role, userRole ?? "Client")
                    };

                    var identity = new ClaimsIdentity(claims, "CustomAuth");
                    _currentUser = new ClaimsPrincipal(identity);

                    // Обновляем глобальное состояние
                    _appStateService.IsAuthenticated = true;
                    _appStateService.UserName = userName ?? "";
                }
                else
                {
                    _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                    _appStateService.IsAuthenticated = false;
                    _appStateService.UserName = "";
                }
            }
            catch
            {
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                _appStateService.IsAuthenticated = false;
                _appStateService.UserName = "";
            }

            return new AuthenticationState(_currentUser);
        }

        public void NotifyUserLogin(string userId, string userEmail, string userName, string userRole)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, userRole)
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            _currentUser = new ClaimsPrincipal(identity);

            _appStateService.IsAuthenticated = true;
            _appStateService.UserName = userName;

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        public void NotifyUserLogout()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            _appStateService.IsAuthenticated = false;
            _appStateService.UserName = "";
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }
}