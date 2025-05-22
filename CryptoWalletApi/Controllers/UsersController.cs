using System.Net;
using AutoMapper;
using CryptoWalletApi.Data;
using CryptoWalletApi.DTO;
using CryptoWalletApi.Models;
using Microsoft.AspNetCore.Mvc;




namespace CryptoWalletApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IMapper _mapper;

        private readonly ILogger<UsersController> _logger;

        public UsersController(ApiContext context, IMapper mapper, ILogger<UsersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }



        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var users = _context.Users.ToList();
            var usersDto = _mapper.Map<List<UserResponseDTO>>(users);
            var response = new ApiResponse()
            {
                IsSuccess = true,
                Result = usersDto,
                StatusCode = HttpStatusCode.OK
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public IActionResult Get(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            var userDto = _mapper.Map<UserResponseDTO>(user);
            var response = new ApiResponse();
            if (userDto is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new List<string> { $"User {id} not found" };
                return NotFound(response);
            }

            response.IsSuccess = true;
            response.Result = userDto;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }


        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public IActionResult Create([FromBody] UserCreateDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList()
                });
            }

            try
            {
                var user = _mapper.Map<User>(request);

                var nextId = GetNextUserId();

                user.Id = nextId;

                var userWallet = new Wallet
                {
                    Id = nextId,
                    UserId = user.Id,
                    User = user
                };

                var defaultCrypto = new CryptoBalance
                {
                    Id = nextId,
                    WalletId = userWallet.Id,
                    Currency = "BTC",
                    Amount = 2
                };

                userWallet.CryptoBalances.Add(defaultCrypto);
                user.Wallet = userWallet;

                _context.Users.Add(user);
                _context.SaveChanges();

                var userDto = _mapper.Map<UserResponseDTO>(user);

                return CreatedAtAction(nameof(Get), new { id = user.Id },
                    new ApiResponse
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
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        ErrorMessages = new List<string> { "An error occurred while creating the user." }
                    });
            }
        }

        private int GetNextUserId()
        {
            if (!_context.Users.Any())
            {
                return 1;
            }

            return _context.Users.Max(u => u.Id) + 1;
        }


        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public IActionResult Edit([FromBody] UserUpdateDTO request)
        {
            var response = new ApiResponse();

            if (!ModelState.IsValid)
            {


                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ErrorMessages = ["Dados inválidos."]
                ;

                return BadRequest(response);
            }

            var user = _context.Users.Find(request.Id);

            if (user == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = ["Usuário não encontrado."];
                return NotFound(response);
            }

            // request => user
            _mapper.Map(request, user);

            _context.SaveChanges();

            var userDto = _mapper.Map<UserResponseDTO>(user);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = userDto;
            return Ok(response);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)] // invalid ID
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            var response = new ApiResponse();

            if (id <= 0)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ErrorMessages = new List<string> { "Invalid id" };
                return BadRequest(response);
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new List<string> { $"User id:{id} not found." };
                return NotFound(response);
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = new { DeletedUserId = id, Message = "User removed." };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}

