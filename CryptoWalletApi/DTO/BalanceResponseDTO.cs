using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletApi.DTO
{
    public class BalanceResponseDTO
    {
        public int Id { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
    }

}