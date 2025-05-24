using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoWalletApi.Models;

namespace CryptoWalletApi.DTO
{
    public class WalletDTO
    {
        public int Id { get; set; }
        public List<BalanceResponseDTO> CryptoBalances { get; set; } = new();

    }
}