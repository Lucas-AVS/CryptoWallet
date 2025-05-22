using System.ComponentModel.DataAnnotations;
using CryptoWalletApi.Models;


namespace CryptoWalletApi.DTO
{
    public class TransactionCreateDTO
    {
        [Required]
        public int ReceiverWalletId { get; set; }

        [Required]
        public int SenderWalletId { get; set; }

        [Required, StringLength(10)]
        public string Currency { get; set; }

        [Required, Range(0.00000001, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}