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
        private readonly CryptoWalletDbContext _context;
        private readonly IMapper _mapper;

        private readonly ILogger<CryptoBalancesController> _logger;

        public CryptoBalancesController(CryptoWalletDbContext context, IMapper mapper, ILogger<CryptoBalancesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BalanceResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<BalanceResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BalanceResponseDTO>>> AddOrUpdateCryptoBalance([FromBody] BalanceCreateDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Currency) || request.Amount <= 0)
            {
                return BadRequest(new ApiResponse<BalanceResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Invalid currency or amount" }
                });
            }

            var user = await _context.Users.Include(u => u.Wallet)
                                     .ThenInclude(w => w.CryptoBalances)
                                     .FirstOrDefaultAsync(u => u.Id == request.UserId);

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

            var currencyUpper = request.Currency.ToUpperInvariant();
            var existingBalance = wallet.CryptoBalances
                                        .FirstOrDefault(cb => cb.Currency == currencyUpper);

            if (existingBalance != null)
            {
                existingBalance.Amount += request.Amount;

                await _context.SaveChangesAsync();

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
                await _context.SaveChangesAsync();

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
