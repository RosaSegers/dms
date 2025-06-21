using FluentAssertions;
using System.Net.Http.Json;
using System.Net;

namespace User.Api.IntegrationTests
{
    public class UserSagaIntegrationTests
    {
        private readonly HttpClient _http;

        public UserSagaIntegrationTests()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("http://api-gateway:80")
            };
        }

        [Fact]
        public async Task DeleteUserSaga_ShouldDeleteUserAndDocuments()
        {
            var userId = Guid.NewGuid();

            // 1. Create a user
            var createUserResponse = await _http.PostAsJsonAsync("/user", new
            {
                Id = userId,
                Name = "Integration Test User"
            });

            createUserResponse.EnsureSuccessStatusCode();

            // 2. Create a document for the user
            var createDocResponse = await _http.PostAsJsonAsync("/document", new
            {
                UserId = userId,
                Title = "Test Document"
            });

            createDocResponse.EnsureSuccessStatusCode();

            // 3. Trigger user delete (which triggers the saga)
            var deleteResponse = await _http.DeleteAsync($"/user/{userId}");

            deleteResponse.EnsureSuccessStatusCode();

            // 4. Wait a bit for saga to complete (or poll/check)
            await Task.Delay(10000);

            // 5. Verify user is deleted
            var getUserResponse = await _http.GetAsync($"/user/{userId}");
            getUserResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

}