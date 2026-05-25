using Microsoft.JSInterop;
using StudioFlow.Models;

namespace StudioFlow.Services
{
    public class AuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly AppStateService _appStateService;

        public AuthService(IJSRuntime jsRuntime, AppStateService appStateService)
        {
            _jsRuntime = jsRuntime;
            _appStateService = appStateService;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var userId = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userId");
                return !string.IsNullOrEmpty(userId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var userId = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userId");
                if (string.IsNullOrEmpty(userId)) return null;

                var userEmail = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userEmail");
                var userName = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userName");
                var userRole = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userRole");

                return new User
                {
                    UserId = int.Parse(userId),
                    Email = userEmail ?? "",
                    FullName = userName ?? "",
                    Role = new Role { RoleName = userRole ?? "Client" }
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task RestoreAuthStateAsync()
        {
            var isAuth = await IsAuthenticatedAsync();
            if (isAuth)
            {
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    _appStateService.IsAuthenticated = true;
                    _appStateService.UserName = user.FullName;
                    _appStateService.UserRole = user.Role?.RoleName ?? "Client";
                }
            }
            else
            {
                _appStateService.IsAuthenticated = false;
                _appStateService.UserName = "";
                _appStateService.UserRole = "";
            }
        }
    }
}