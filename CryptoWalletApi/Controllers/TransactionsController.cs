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
        private readonly ApiContext _context;
        private readonly IMapper _mapper;

        public TransactionsController(ApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public IActionResult Get(int id)
        {
            var transaction = _context.Transactions.FirstOrDefault(u => u.Id == id);
            var transactionDto = _mapper.Map<TransactionResponseDTO>(transaction);
            var response = new ApiResponse();
            if (transactionDto is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new List<string> { $"transaction {id} not found" };
                return NotFound(response);
            }

            response.IsSuccess = true;
            response.Result = transactionDto;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public IActionResult Create([FromBody] TransactionCreateDTO request)
        {
            var response = new ApiResponse()
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest
            };
            if (!ModelState.IsValid)
            {
                return BadRequest(response);
            }

            var senderWallet = _context.Wallets
                .Include(w => w.CryptoBalances)
                .FirstOrDefault(w => w.Id == request.SenderWalletId);

            var senderCryptoBalance = senderWallet.CryptoBalances.Where(cb => cb.Currency == request.Currency).FirstOrDefault();
            if (senderCryptoBalance.Amount < request.Amount)
            {
                response.ErrorMessages = new List<string> { $"Sender doesn't have {request.Amount} {request.Currency}" };
                return BadRequest(response);
            }

            var receiverWallet = _context.Wallets
                .Include(w => w.CryptoBalances)
                .FirstOrDefault(w => w.Id == request.ReceiverWalletId);

            var receiverCryptoBalance = senderWallet.CryptoBalances.Where(cb => cb.Currency == request.Currency).FirstOrDefault();

            receiverCryptoBalance.Amount += request.Amount;
            senderCryptoBalance.Amount -= request.Amount;

            var transaction = new Transaction()
            {
                Id = 1,
                SenderWalletId = senderWallet.Id,
                SenderWallet = senderWallet,
                ReceiverWalletId = receiverWallet.Id,
                ReceiverWallet = receiverWallet,
                Currency = request.Currency,
                Amount = request.Amount,
                Timestamp = DateTime.Now
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            // var transaction = _mapper.Map<Transaction>(request);
            var transactionDto = _mapper.Map<TransactionResponseDTO>(transaction);
            response.Result = transactionDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;

            return CreatedAtAction(nameof(Get), new { id = transaction.Id }, response);
        }

        private int GetNextTransactionId()
        {
            if (!_context.Transactions.Any())
            {
                return 1;
            }

            return _context.Transactions.Max(u => u.Id) + 1;
        }
    }

}