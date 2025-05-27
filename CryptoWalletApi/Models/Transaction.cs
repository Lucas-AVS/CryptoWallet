using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletApi.Models
{
    [Table("transaction")]
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("sender_wallet_id")]
        public int SenderWalletId { get; set; }

        [ForeignKey("SenderWalletId")]
        public Wallet SenderWallet { get; set; }

        [Column("receiver_wallet_id")]
        public int ReceiverWalletId { get; set; }

        [ForeignKey("ReceiverWalletId")]
        public Wallet ReceiverWallet { get; set; }

        [Column("currency")]
        public string Currency { get; set; } // e.g. "BTC", "ETH"

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}