namespace CryptoWalletApi.Models
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }

}