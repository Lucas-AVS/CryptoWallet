using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CryptoWalletApi.DTO;
using CryptoWalletApi.Models;
using System.Net;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        // _factory = factory;
        _client = factory.CreateClient();
    }


    // Happy Path
    [Fact]
    public async Task PostUser_WithValidData_Returns201CreatedAndUserData()
    {
        // Arrange
        var newUserDto = new UserCreateDTO { Email = "post.created@example.com", Password = "Post123", Name = "Post User" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", newUserDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode); // 201

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.IsSuccess);
        Assert.NotNull(apiResponse.Result); // UserResponseDTO is inside Result

        var createdUser = apiResponse.Result;
        Assert.Equal(newUserDto.Name, createdUser.Name);
        Assert.Equal(newUserDto.Email, createdUser.Email);
        Assert.NotEqual(0, createdUser.Id);
    }

    [Fact]
    public async Task GetUsers_WhenUsersExist_Returns200OKAndListOfUsers()
    {
        // Arrange
        var uniqueEmail = "get.users." + Guid.NewGuid().ToString() + "@example.com";
        var userToCreate = new UserCreateDTO { Email = uniqueEmail, Password = "Password123", Name = "Get Users Test" };
        var postResponse = await _client.PostAsJsonAsync("/api/users", userToCreate);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);  // Ensures that it was created
        var postApiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(postApiResponse?.Result); // Check for response
        var createdUserId = postApiResponse.Result.Id;

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // 200
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserResponseDTO>>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.IsSuccess);
        Assert.NotNull(apiResponse.Result);

        var users = apiResponse.Result;
        Assert.True(users.Any(u => u.Id == createdUserId && u.Email == uniqueEmail)); // Check for created user
    }

    [Fact]
    public async Task GetUserById_WhenUserExists_Returns200OKAndCorrectUser()
    {
        // Arrange
        var uniqueEmail = "get.user.by.id." + Guid.NewGuid().ToString() + "@example.com";
        var newUserDto = new UserCreateDTO { Email = uniqueEmail, Password = "Password123", Name = "Get User By Id Test" };
        var postResponse = await _client.PostAsJsonAsync("/api/users", newUserDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode); // Ensures that it was created
        var postApiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(postApiResponse?.Result); // Check for response
        var userIdToFetch = postApiResponse.Result.Id; // Get the ID of the newly created user


        // Act
        var response = await _client.GetAsync($"/api/users/{userIdToFetch}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.IsSuccess);
        Assert.NotNull(apiResponse.Result);

        var fetchedUser = apiResponse.Result;
        Assert.Equal(userIdToFetch, fetchedUser.Id);
        Assert.Equal(newUserDto.Name, fetchedUser.Name);
        Assert.Equal(newUserDto.Email, fetchedUser.Email);
    }

    [Fact]
    public async Task UpdateUser_WhenUserExistsAndDataIsValid_Returns200OKAndUpdatedUser()
    {
        // Arrange - Part 1: Create user
        var initialEmail = "user.to.update.for.put@example.com";
        var initialUserDto = new UserCreateDTO { Email = initialEmail, Password = "Password123", Name = "Original Name" };
        var createResponse = await _client.PostAsJsonAsync("/api/users", initialUserDto);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdUserApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>(); // ApiResponse
        Assert.NotNull(createdUserApiResponse);
        Assert.True(createdUserApiResponse.IsSuccess);
        Assert.NotNull(createdUserApiResponse.Result);
        var createdUserId = createdUserApiResponse.Result.Id; // Save ID for later verification

        // Arrange - Part 2: Update user by email
        var updateUserPayload = new UserUpdateDTO
        {
            Email = initialEmail, // Email para identificar o usuário
            Name = "Updated Name",
            NewPassword = "UpdatedPassword123"
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync("/api/users", updateUserPayload);

        // Assert - Verificar a resposta da atualização
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedUserApiResponse = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(updatedUserApiResponse);
        Assert.True(updatedUserApiResponse.IsSuccess);
        Assert.NotNull(updatedUserApiResponse.Result);
        Assert.Equal(createdUserId, updatedUserApiResponse.Result.Id); // check ID
        Assert.Equal("Updated Name", updatedUserApiResponse.Result.Name);
        Assert.Equal(initialEmail, updatedUserApiResponse.Result.Email); // email is the identifier in put method

        // Assert - Parte 2
        // Check data persistence
        var getResponse = await _client.GetAsync($"/api/users/{createdUserId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var apiResponseGet = await getResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(apiResponseGet);
        Assert.True(apiResponseGet.IsSuccess);
        Assert.NotNull(apiResponseGet.Result);
        Assert.Equal("Updated Name", apiResponseGet.Result.Name);
        Assert.Equal(initialEmail, apiResponseGet.Result.Email);
    }

    [Fact]
    public async Task DeleteUserById_WhenUserExists_ReturnsOkAndUserIsDeleted() // Ajuste o nome e o status code esperado se necessário
    {
        // Arrange
        var uniqueEmail = "delete.user." + Guid.NewGuid().ToString() + "@example.com";
        var newUserDto = new UserCreateDTO { Email = uniqueEmail, Password = "D.Password123", Name = "Delete User" };
        var postResponse = await _client.PostAsJsonAsync("/api/users", newUserDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);  // Ensures that it was created
        var postApiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDTO>>();
        Assert.NotNull(postApiResponse?.Result);  // Check for response
        var userIdToDelete = postApiResponse.Result.Id;  // Get the ID of the newly created user

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/users/{userIdToDelete}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleteApiResponse = await deleteResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(deleteApiResponse);
        Assert.True(deleteApiResponse.IsSuccess);

        // Check if user is removed
        var getResponseAfterDelete = await _client.GetAsync($"/api/users/{userIdToDelete}");
        Assert.Equal(HttpStatusCode.NotFound, getResponseAfterDelete.StatusCode);
    }

    // ... TODO: Unhappy Path
}