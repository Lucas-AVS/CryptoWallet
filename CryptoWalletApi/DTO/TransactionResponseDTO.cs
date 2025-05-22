using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletApi.DTO
{
    public class TransactionResponseDTO
    {
        public int Id { get; set; }
        public int ReceiverWalletId { get; set; }
        public int SenderWalletId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        // public string Status { get; set; } // Ex: "Completed", "Failed"
    }
}