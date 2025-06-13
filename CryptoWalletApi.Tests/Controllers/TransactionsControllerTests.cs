using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CryptoWalletApi.DTO;
using CryptoWalletApi.Models;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;

public class TransactionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // --- Private Helper Methods to make tests cleaner ---

    private async Task<UserResponseDTO> CreateTestUserAsync(string namePrefix)
    {
        string uniqueEmail = $"{namePrefix}.{Guid.NewGuid()}@example.com";
        var userToCreate = new UserCreateDTO { Name = $"{namePrefix} User", Email = uniqueEmail, Password = "Password123" };

        var response = await _client.PostAsJsonAsync("/api/users", userToCreate);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(apiResponse?.Result);
        return apiResponse.Result;
    }

    private async Task AddFundsToUserAsync(int userId, string currency, decimal amount)
    {
        var payload = new BalanceCreateDTO() { UserId = userId, Currency = currency, Amount = amount };
        var response = await _client.PostAsJsonAsync("/api/cryptobalances", payload);

        // This endpoint returns 201 for new currency or 200 for updating existing. Both are success.
        response.EnsureSuccessStatusCode();
    }

    // --- Happy Path Tests ---

    [Fact]
    public async Task GetTransactionById_WhenTransactionExists_Returns200OKAndCorrectData()
    {
        // Arrange
        // 1. Create a sender and receiver
        var sender = await CreateTestUserAsync("sender");
        var receiver = await CreateTestUserAsync("receiver");

        // 2. Add funds to the sender's wallet
        await AddFundsToUserAsync(sender.Id, "ETH", 10m);

        // 3. Create the transaction
        var transactionPayload = new TransactionCreateDTO
        {
            SenderWalletId = sender.Wallet.Id,
            ReceiverWalletId = receiver.Wallet.Id,
            Currency = "ETH",
            Amount = 5m
        };
        var createTransactionResponse = await _client.PostAsJsonAsync("/api/transactions", transactionPayload);
        Assert.Equal(HttpStatusCode.Created, createTransactionResponse.StatusCode);
        var transactionApiResponse = await createTransactionResponse.Content.ReadFromJsonAsync<ApiResponse<TransactionResponseDTO>>();
        Assert.NotNull(transactionApiResponse?.Result);
        var transactionIdToFetch = transactionApiResponse.Result.Id;

        // Act
        var getResponse = await _client.GetAsync($"/api/transactions/{transactionIdToFetch}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var getApiResponse = await getResponse.Content.ReadFromJsonAsync<ApiResponse<TransactionResponseDTO>>();
        Assert.NotNull(getApiResponse?.Result);
        Assert.True(getApiResponse.IsSuccess);

        var fetchedTransaction = getApiResponse.Result;
        Assert.Equal(transactionIdToFetch, fetchedTransaction.Id);
        Assert.Equal(receiver.Wallet.Id, fetchedTransaction.ReceiverWalletId);
        Assert.Equal(sender.Wallet.Id, fetchedTransaction.SenderWalletId);
        Assert.Equal(5m, fetchedTransaction.Amount);
        Assert.Equal("ETH", fetchedTransaction.Currency);
    }
}
