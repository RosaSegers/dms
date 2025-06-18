using DocumentFrontend.Models;
using System.Net.Http.Headers;

namespace DocumentFrontend.Services
{
    public class AuthService(IHttpClientFactory clientFactory)
    {
        public async Task<AuthResponse?> LoginAsync(LoginRequest login)
        {
            var client = clientFactory.CreateClient("Unauthenticated");

            var formData = new MultipartFormDataContent
            {
                { new StringContent(login.Email), "Email" },
                { new StringContent(login.Password), "Password" }
            };
            
            var response = await client.PostAsync("gateway/auth/login", formData);

            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Login response: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            }

            return null;
        }


        public async Task<AuthResponse?> RegisterAsync(RegisterRequest register)
        {
            var client = clientFactory.CreateClient("Unauthenticated");

            var formData = new MultipartFormDataContent
            {
                { new StringContent(register.Username), "username" },
                { new StringContent(register.Email), "email" },
                { new StringContent(register.Password), "password" }
            };

            var response = await client.PostAsync("gateway/auth/register", formData);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            }

            return null;
        }

        public Task<UserModel> GetCurrentUserAsync()
        {
            var dummyUser = new UserModel
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            };

            return Task.FromResult(dummyUser);
        }

        public Task<bool> UpdateUserAsync(UserModel updatedUser)
        {

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
