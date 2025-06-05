using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CryptoWalletApi.DTO;
using CryptoWalletApi.Models;
using System.Net;

public class WalletsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public WalletsControllerTests(CustomWebApplicationFactory factory)
    {
        // _factory = factory;
        _client = factory.CreateClient();
    }


    // Happy Path
    [Fact]
    public async Task GetWallets_WhenWalletsExist_Returns200OKAndListOfWallets()
    {
        // Arrange
        var uniqueEmail = "get.users." + Guid.NewGuid().ToString() + "@example.com";
        var userToCreate = new UserCreateDTO { Email = uniqueEmail, Password = "Password123", Name = "Get Users Test" };
        var postResponse = await _client.PostAsJsonAsync("/api/users", userToCreate);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);  // Ensures that user was created
        var postApiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(postApiResponse?.Result); // Check for response
        var createdUserWalletId = postApiResponse.Result.Wallet.Id;

        // Act
        var response = await _client.GetAsync("/api/wallets");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // 200
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<WalletDTO>>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.IsSuccess);
        Assert.NotNull(apiResponse.Result);

        var wallets = apiResponse.Result;
        Assert.True(wallets.Any(w => w.Id == createdUserWalletId)); // Check for created user wallet
    }

    [Fact]
    public async Task GetWalletById_WhenWalletExists_Returns200OKAndCorrectWallet()
    {
        // Arrange
        var uniqueEmail = "get.users." + Guid.NewGuid().ToString() + "@example.com";
        var userToCreate = new UserCreateDTO { Email = uniqueEmail, Password = "Password123", Name = "Get Users Test" };
        var postResponse = await _client.PostAsJsonAsync("/api/users", userToCreate);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);  // Ensures that user was created
        var postApiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(postApiResponse?.Result); // Check for response
        var walletIdToFetch = postApiResponse.Result.Wallet.Id;

        // Act
        var response = await _client.GetAsync($"/api/wallets/{walletIdToFetch}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<WalletDTO>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.IsSuccess);
        Assert.NotNull(apiResponse.Result);

        var fetchedWallet = apiResponse.Result;
        Assert.Equal(walletIdToFetch, fetchedWallet.Id);

        // Verify default crypto balance values (2 BTC)
        Assert.NotNull(fetchedWallet.CryptoBalances);
        Assert.NotEmpty(fetchedWallet.CryptoBalances);
        var btcBalance = fetchedWallet.CryptoBalances.FirstOrDefault(cb => cb.Currency == "BTC");
        Assert.NotNull(btcBalance);
        Assert.Equal(2, btcBalance.Amount);
    }
}