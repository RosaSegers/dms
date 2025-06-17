using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net;

namespace DocumentFrontend.Services
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly TokenService _tokenService;
        private readonly NavigationManager _nav;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthHandler(TokenService tokenService, NavigationManager nav, IHttpClientFactory factory)
        {
            _tokenService = tokenService;
            _nav = nav;
            _httpClientFactory = factory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Try to refresh token
                var refreshToken = await _tokenService.GetRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    await _tokenService.ClearTokensAsync();
                    _nav.NavigateTo("/login");
                    return response;
                }

                var client = _httpClientFactory.CreateClient("Unauthenticated"); 
                var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

                if (refreshResponse.IsSuccessStatusCode)
                {
                    var newTokens = await refreshResponse.Content.ReadFromJsonAsync<AuthResult>();
                    await _tokenService.SetTokensAsync(newTokens.Token, newTokens.RefreshToken);

                    // Retry original request with new token
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.Token);
                    return await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    await _tokenService.ClearTokensAsync();
                    _nav.NavigateTo("/login");
                }
            }

            return response;
        }

        public class AuthResult
        {
            public string Token { get; set; }
            public string RefreshToken { get; set; }
        }
    }

}
