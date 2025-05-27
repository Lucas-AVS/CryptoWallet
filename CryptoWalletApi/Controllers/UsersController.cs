using System.Net;
using AutoMapper;
using CryptoWalletApi.Data;
using CryptoWalletApi.DTO;
using CryptoWalletApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;




namespace CryptoWalletApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly CryptoWalletDbContext _context;
        private readonly IMapper _mapper;

        private readonly ILogger<UsersController> _logger;

        public UsersController(CryptoWalletDbContext context, IMapper mapper, ILogger<UsersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }



        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserResponseDTO>>), StatusCodes.Status200OK)]

        public async Task<ActionResult<ApiResponse<List<UserResponseDTO>>>> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            var usersDto = _mapper.Map<List<UserResponseDTO>>(users);

            var response = new ApiResponse<List<UserResponseDTO>>()
            {
                IsSuccess = true,
                Result = usersDto,
                StatusCode = HttpStatusCode.OK
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> Get(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                return NotFound(new ApiResponse<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { $"User {id} not found" }
                });
            }

            var userDto = _mapper.Map<UserResponseDTO>(user);

            return Ok(new ApiResponse<UserResponseDTO>
            {
                IsSuccess = true,
                Result = userDto,
                StatusCode = HttpStatusCode.OK
            });
        }


        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> Create([FromBody] UserCreateDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList()
                });
            }
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new ApiResponse<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Email already in use" }
                });
            }

            try
            {
                var user = _mapper.Map<User>(request);
                user.Wallet = new Wallet
                {
                    CryptoBalances = new List<CryptoBalance> // test funds 
                        {
                            new CryptoBalance
                            {
                                Currency = "BTC",
                                Amount = 2
                            }
                        }
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserResponseDTO>(user);

                return CreatedAtAction(nameof(Get), new { id = user.Id },
                    new ApiResponse<UserResponseDTO>
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.Created,
                        Result = userDto
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        ErrorMessages = new List<string> { ex.Message }
                    });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> Edit([FromBody] UserUpdateDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Invalid data." }
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return NotFound(new ApiResponse<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "User not found" }
                });
            }

            _mapper.Map(request, user);
            await _context.SaveChangesAsync();

            var userDto = _mapper.Map<UserResponseDTO>(user);

            return Ok(new ApiResponse<UserResponseDTO>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = userDto
            });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Attempt to delete user with invalid ID: {UserId}", id);
                return BadRequest(new ApiResponse<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Invalid id" }
                });
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Attempt to delete non-existent user with ID: {UserId}", id);
                    return NotFound(new ApiResponse<UserResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { $"User id:{id} not found." }
                    });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User with ID: {UserId} successfully deleted.", id);

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = new { DeletedUserId = id, Message = "User removed." }
                });
            }
            catch (DbUpdateException dbEx) // EFC Exceptions
            {
                _logger.LogError(dbEx, "Database error occurred while deleting user {UserId}. Possible FK constraint?", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        ErrorMessages = new List<string> { "A database error occurred while trying to delete the user. It might be linked to other data." }
                    });
            }
            catch (Exception ex) // Other erros
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        ErrorMessages = new List<string> { $"An unexpected error occurred: {ex.Message}" }
                    });
            }
        }
    }
}

