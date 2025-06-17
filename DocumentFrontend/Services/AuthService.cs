using DocumentFrontend.Models;
using System.Net.Http.Headers;

namespace DocumentFrontend.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _clientFactory;

        public AuthService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest login)
        {
            var client = _clientFactory.CreateClient("Unauthenticated");
            var response = await client.PostAsJsonAsync("/gateway/auth/login", login);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            }

            return null;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest register)
        {
            var client = _clientFactory.CreateClient("Unauthenticated");

            var formData = new MultipartFormDataContent
            {
                { new StringContent(register.Username), "username" },
                { new StringContent(register.Email), "email" },
                { new StringContent(register.Password), "password" }
            };

            var response = await client.PostAsync("/gateway/auth/register", formData);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            }

            return null;
        }

        // Dummy method to get current user (for testing)
        public Task<UserModel> GetCurrentUserAsync()
        {
            var dummyUser = new UserModel
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
                // Add other properties if needed
            };

            return Task.FromResult(dummyUser);
        }

        // Dummy method to update user info
        public Task<bool> UpdateUserAsync(UserModel updatedUser)
        {
            // Simulate async update operation
            // In real use, send HTTP PUT or PATCH request here

            // Return true to indicate success
            return Task.FromResult(true);
        }

        // Dummy method to delete user account
        public Task<bool> DeleteUserAsync(Guid userId)
        {
            // Simulate async delete operation
            // In real use, send HTTP DELETE request here

            // Return true to indicate success
            return Task.FromResult(true);
        }
    }
}
