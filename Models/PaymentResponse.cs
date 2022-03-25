using System;
using System.Collections.Generic;

namespace OS_Sog.Models
{
    [Serializable]
    public class PaymentResponse
    {
        public string shopId { get; set; }
        public string orderCycle { get; set; }
        public string orderStatus { get; set; }
        public DateTime serverDate { get; set; }
        public OrderDetails orderDetails { get; set; }
        public Customer customer { get; set; }
        public List<Transaction> transactions { get; set; }
        public string _type { get; set; }
    }

}
