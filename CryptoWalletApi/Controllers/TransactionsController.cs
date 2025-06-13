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
    public class TransactionsController : ControllerBase
    {
        private readonly CryptoWalletDbContext _context;
        private readonly IMapper _mapper;

        public TransactionsController(CryptoWalletDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TransactionResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<TransactionResponseDTO>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TransactionResponseDTO>>> Get(int id)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(u => u.Id == id);
            if (transaction is null)
            {
                return NotFound(new ApiResponse<TransactionResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { $"transaction {id} not found" }
                });
            }
            var transactionDto = _mapper.Map<TransactionResponseDTO>(transaction);
            var response = new ApiResponse<TransactionResponseDTO>();

            response.IsSuccess = true;
            response.Result = transactionDto;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TransactionResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<TransactionResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<TransactionResponseDTO>>> Create([FromBody] TransactionCreateDTO request)
        {
            var response = new ApiResponse<TransactionResponseDTO>()
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(response);
            }

            var senderWallet = await _context.Wallets
                .Include(w => w.CryptoBalances)
                .FirstOrDefaultAsync(w => w.Id == request.SenderWalletId);

            if (senderWallet == null)
            {
                response.ErrorMessages = new List<string> { $"Sender wallet {request.SenderWalletId} not found" };
                return BadRequest(response);
            }

            var senderCryptoBalance = senderWallet.CryptoBalances
                .FirstOrDefault(cb => cb.Currency == request.Currency);

            if (senderCryptoBalance == null || senderCryptoBalance.Amount < request.Amount)
            {
                response.ErrorMessages = new List<string> { $"Sender doesn't have enough {request.Currency}" };
                return BadRequest(response);
            }

            var receiverWallet = await _context.Wallets
                .Include(w => w.CryptoBalances)
                .FirstOrDefaultAsync(w => w.Id == request.ReceiverWalletId);

            if (receiverWallet == null)
            {
                response.ErrorMessages = new List<string> { $"Receiver wallet {request.ReceiverWalletId} not found" };
                return BadRequest(response);
            }

            var receiverCryptoBalance = receiverWallet.CryptoBalances
                .FirstOrDefault(cb => cb.Currency == request.Currency);

            if (receiverCryptoBalance == null)
            // If the user does not have the currency, an empty(0) "cryptobalance" is created before receiving the transfer
            {
                receiverCryptoBalance = new CryptoBalance
                {
                    Currency = request.Currency.ToUpperInvariant(),
                    Amount = 0, //
                    WalletId = receiverWallet.Id
                };
                receiverWallet.CryptoBalances.Add(receiverCryptoBalance);
            }

            receiverCryptoBalance.Amount += request.Amount;
            senderCryptoBalance.Amount -= request.Amount;

            var transaction = new Transaction()
            {
                SenderWalletId = senderWallet.Id,
                ReceiverWalletId = receiverWallet.Id,
                Currency = request.Currency,
                Amount = request.Amount,
                Timestamp = DateTime.UtcNow
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            var transactionDto = _mapper.Map<TransactionResponseDTO>(transaction);
            response.Result = transactionDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;

            return CreatedAtAction(nameof(Get), new { id = transaction.Id }, response);
        }
    }

}