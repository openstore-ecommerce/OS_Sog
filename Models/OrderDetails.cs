namespace OS_Sog.Models
{
    public class OrderDetails
    {
        public int orderTotalAmount { get; set; }
        public string orderCurrency { get; set; }
        public string mode { get; set; }
        public string orderId { get; set; }
        public string _type { get; set; }
    }
}
