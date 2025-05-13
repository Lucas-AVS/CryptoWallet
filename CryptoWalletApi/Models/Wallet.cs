using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletApi.Models
{
    public class Wallet
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public List<CryptoBalance> CryptoBalances { get; set; } = new();
        public List<Transaction> SentTransactions { get; set; } = new();
        public List<Transaction> ReceivedTransactions { get; set; } = new();
    }
}