using Microsoft.JSInterop;

namespace DocumentFrontend.Services
{
    public class TokenService
    {
        private readonly IJSRuntime _js;

        public TokenService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string> GetAccessTokenAsync() =>
            await _js.InvokeAsync<string>("localStorage.getItem", "token");

        public async Task<string> GetRefreshTokenAsync() =>
            await _js.InvokeAsync<string>("localStorage.getItem", "refreshToken");

        public async Task SetTokensAsync(string accessToken, string refreshToken)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "token", accessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", "refreshToken", refreshToken);
        }

        public async Task ClearTokensAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "token");
            await _js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
        }

        public async Task LogoutAsync(ApiAuthenticationStateProvider authProvider)
        {
            await ClearTokensAsync();
            authProvider.NotifyUserLogout();
        }
    }

}
