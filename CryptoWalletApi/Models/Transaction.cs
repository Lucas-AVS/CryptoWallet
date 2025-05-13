using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletApi.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int SenderWalletId { get; set; }
        public Wallet SenderWallet { get; set; }

        public int ReceiverWalletId { get; set; }
        public Wallet ReceiverWallet { get; set; }

        public string Currency { get; set; } // e.g. "BTC", "ETH"
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}