using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletApi.Models
{
    public class CryptoBalance
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public Wallet Wallet { get; set; }
        public string Currency { get; set; } // e.g. "BTC", "ETH"
        public decimal Amount { get; set; }
    }
}