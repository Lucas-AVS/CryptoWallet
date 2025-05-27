using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletApi.Models
{
    [Table("crypto_balance")]
    public class CryptoBalance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("wallet_id")]
        public int WalletId { get; set; }

        [ForeignKey("WalletId")]
        public Wallet Wallet { get; set; }
        public string Currency { get; set; } // e.g. "BTC", "ETH"
        public decimal Amount { get; set; }
    }
}