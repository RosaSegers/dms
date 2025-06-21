using FluentAssertions;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace User.Api.IntegrationTests
{
    public class UserApiIntegrationTests
    {
        private readonly HttpClient _client;

        public UserApiIntegrationTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5001") // API Gateway exposed port
            };
        }

        [Fact]
        public async Task CanCreateAndDeleteUser()
        {
            var userPayload = new MultipartFormDataContent
        {
            { new StringContent("pipelineUser123"), "Username" },
            { new StringContent("pipelineuser123@example.com"), "Email" },
            { new StringContent("Password123456Aa"), "Password" }
        };

            var createResponse = await _client.PostAsync("/api/users", userPayload);
            createResponse.EnsureSuccessStatusCode();

            var userIdString = await createResponse.Content.ReadAsStringAsync();
            var userId = JsonSerializer.Deserialize<string>(userIdString);
            var userGuid = Guid.Parse(userId!);

            var deleteResponse = await _client.DeleteAsync($"/api/users/{userId}");
            deleteResponse.EnsureSuccessStatusCode();
        }
    }


}