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
    public class CryptoBalancesController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IMapper _mapper;

        private readonly ILogger<UsersController> _logger;

        public CryptoBalancesController(ApiContext context, IMapper mapper, ILogger<UsersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BalanceResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<BalanceResponseDTO>), StatusCodes.Status400BadRequest)]
        public IActionResult AddOrUpdateCryptoBalance([FromBody] BalanceCreateDTO request)
        {
            var user = _context.Users.Include(u => u.Wallet)
                                     .ThenInclude(w => w.CryptoBalances)
                                     .FirstOrDefault(u => u.Id == request.UserId);

            if (user == null)
            {
                return NotFound(new ApiResponse<BalanceResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "User not found" }
                });
            }

            var wallet = user.Wallet;
            if (wallet == null)
            {
                return NotFound(new ApiResponse<BalanceResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "Wallet not found" }
                });
            }

            var currencyUpper = request.Currency.ToUpper();
            var existingBalance = wallet.CryptoBalances
                                        .FirstOrDefault(cb => cb.Currency == currencyUpper);

            if (existingBalance != null)
            {
                existingBalance.Amount += request.Amount;

                _context.SaveChanges();

                var updatedDto = _mapper.Map<BalanceResponseDTO>(existingBalance);

                return Ok(new ApiResponse<BalanceResponseDTO>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = updatedDto
                });
            }
            else
            {
                var newBalance = new CryptoBalance
                {
                    Currency = currencyUpper,
                    Amount = request.Amount,
                    WalletId = wallet.Id
                };

                _context.CryptoBalances.Add(newBalance);
                _context.SaveChanges();

                var newDto = _mapper.Map<BalanceResponseDTO>(newBalance);

                return StatusCode(StatusCodes.Status201Created, new ApiResponse<BalanceResponseDTO>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Created,
                    Result = newDto
                });
            }
        }

    }
}
