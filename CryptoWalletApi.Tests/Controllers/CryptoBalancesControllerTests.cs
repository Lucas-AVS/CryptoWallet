using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CryptoWalletApi.DTO;
using CryptoWalletApi.Models;
using System.Net;

public class CryptoBalancesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CryptoBalancesControllerTests(CustomWebApplicationFactory factory)
    {
        // _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostCryptoBalanceByUserId_WhenUserExists_Returns201CreatedAndCorrectCryptoBalance()
    {
        // Arrange
        // Create user
        var uniqueEmail = "get.users." + Guid.NewGuid().ToString() + "@example.com";
        var userToCreate = new UserCreateDTO { Email = uniqueEmail, Password = "Password123", Name = "Get Users Test" };
        var postResponse = await _client.PostAsJsonAsync("/api/users", userToCreate);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);  // Ensures that user was created
        var postApiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(postApiResponse?.Result); // Check for response
        var newCryptoBalance = new BalanceCreateDTO()
        {
            UserId = postApiResponse.Result.Id,
            Currency = "ETH",
            Amount = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cryptobalances/", newCryptoBalance);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<BalanceResponseDTO>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.IsSuccess);
        Assert.NotNull(apiResponse.Result);

        var fetchedCryptoCurrency = apiResponse.Result;
        Assert.NotNull(fetchedCryptoCurrency);
        Assert.NotEmpty(fetchedCryptoCurrency.Currency);
        Assert.Equal("ETH", fetchedCryptoCurrency.Currency);
        Assert.Equal(10, fetchedCryptoCurrency.Amount);
    }


    [Fact]
    public async Task AddCryptoBalanceByUserId_WhenCurrencyAlreadyExistInUsersWallet_Returns200OKAndCorrectCryptoBalance()
    {
        // Arrange
        // When user is created it gets 2 amount of BTC currency, we will try to add more 2 into his wallet
        // first create the user
        var uniqueEmail = "get.users." + Guid.NewGuid().ToString() + "@example.com";
        var userToCreate = new UserCreateDTO { Email = uniqueEmail, Password = "Password123", Name = "Get Users Test" };
        var postResponse = await _client.PostAsJsonAsync("/api/users", userToCreate);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);  // Ensures that user was created
        var postApiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(postApiResponse?.Result); // Check for response
        var newCryptoBalance = new BalanceCreateDTO()
        {
            UserId = postApiResponse.Result.Id,
            Currency = "BTC",
            Amount = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cryptobalances/", newCryptoBalance);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // 200 OK
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<BalanceResponseDTO>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.IsSuccess);
        Assert.NotNull(apiResponse.Result);

        var fetchedCryptoCurrency = apiResponse.Result;
        Assert.NotNull(fetchedCryptoCurrency);
        Assert.NotEmpty(fetchedCryptoCurrency.Currency);
        Assert.Equal("BTC", fetchedCryptoCurrency.Currency);
        Assert.Equal(4, fetchedCryptoCurrency.Amount);
    }
}
