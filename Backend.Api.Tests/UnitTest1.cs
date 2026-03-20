using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

namespace Backend.Api.Tests
{
    public class UnitTest1
    {
        private sealed class BackendFactory : WebApplicationFactory<Backend.Api.Program>
        {
        }

        private sealed class TokenResponse
        {
            public string token { get; set; } = string.Empty;
        }

        [Fact]
        public async Task Login_Ok_ReturnsToken()
        {
            using var factory = new BackendFactory();
            using var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/auth/login",
                new { username = "admin", password = "Admin123!" }
            );

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.NotNull(body);
            Assert.False(string.IsNullOrWhiteSpace(body!.token));
        }

        [Fact]
        public async Task GetData_WithoutToken_Returns401()
        {
            using var factory = new BackendFactory();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/data");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}