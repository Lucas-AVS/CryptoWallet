namespace CryptoWalletApi.DTO
{
    public class BalanceCreateDTO
    {
        public int UserId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}