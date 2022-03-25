namespace OS_Sog.Models
{

    public class TokenRequest
    {
        public long amount { get; set; }
        public string currency { get; set; }
        public string orderId { get; set; }
        public Customer customer { get; set; }
    }
}
