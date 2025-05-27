
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
    public class WalletsController : ControllerBase
    {
        private readonly CryptoWalletDbContext _context;
        private readonly IMapper _mapper;

        private readonly ILogger<WalletsController> _logger;

        public WalletsController(CryptoWalletDbContext context, IMapper mapper, ILogger<WalletsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }



        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<WalletDTO>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<WalletDTO>>> GetAll()
        {
            var wallets = await _context.Wallets.Include(w => w.CryptoBalances).ToListAsync();
            var walletsDto = _mapper.Map<List<WalletDTO>>(wallets);

            var response = new ApiResponse<List<WalletDTO>>()
            {
                IsSuccess = true,
                Result = walletsDto,
                StatusCode = HttpStatusCode.OK
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<WalletDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<WalletDTO>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<WalletDTO>>> Get(int id)
        {
            var wallet = await _context.Wallets
                .Include(w => w.CryptoBalances)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wallet is null)
            {
                return NotFound(new ApiResponse<WalletDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { $"Wallet {id} not found" }
                });
            }

            var walletDto = _mapper.Map<WalletDTO>(wallet);

            return Ok(new ApiResponse<WalletDTO>
            {
                IsSuccess = true,
                Result = walletDto,
                StatusCode = HttpStatusCode.OK
            });
        }

    }
}