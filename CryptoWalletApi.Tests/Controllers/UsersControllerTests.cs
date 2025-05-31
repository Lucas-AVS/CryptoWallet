using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CryptoWalletApi.DTO;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        // _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newUser = new UserCreateDTO { Email = "test@example.com", Password = "Password123", Name = "Test User" };
        // Act
        var response = await _client.PostAsJsonAsync("/api/users", newUser);
        // Assert
        response.EnsureSuccessStatusCode(); // Throws exception if not 2xx
    }

    // ... TODO: other tests> Get, Put, Delete, Login, error scenarios, etc.
}