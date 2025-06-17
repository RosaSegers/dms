using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net;
using DocumentFrontend.Models;

namespace DocumentFrontend.Services
{
    public class AuthHandler(
        ITokenCache tokenCache,
        IHttpClientFactory factory,
        NavigationManager nav,
        ApiAuthenticationStateProvider authStateProvider)
        : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = tokenCache.AccessToken;
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = tokenCache.RefreshToken;
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    nav.NavigateTo("/login");
                    return response;
                }

                var client = factory.CreateClient("Unauthenticated");

                var content = new MultipartFormDataContent
                {
                    { new StringContent(refreshToken), "refreshToken" }
                };

                var refreshResponse = await client.PostAsync("gateway/auth/refresh", content);

                if (refreshResponse.IsSuccessStatusCode)
                {
                    var result = await refreshResponse.Content.ReadFromJsonAsync<AuthResult>();
                    tokenCache.AccessToken = result.AccessToken;
                    tokenCache.RefreshToken = result.RefreshToken;
                    authStateProvider.NotifyUserAuthentication(result.AccessToken);

                    // Retry original request with new token
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    return await base.SendAsync(request, cancellationToken);
                }

                nav.NavigateTo("/login");
            }

            return response;
        }



        public class AuthResult
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }
    }

}
