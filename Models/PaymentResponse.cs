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



        //public int shopId { get; set; }
        //public string orderCycle { get; set; }
        //public string orderStatus { get; set; }
        //public OrderDetails orderDetails { get; set; }
        //public List<Transaction> Transactions { get; set; }
        //public Customer customer { get; set; }

        //public string webService { get; set; }
        //public string version { get; set; }
        //public string applicationVersion { get; set; }
        //public string status { get; set; }
        //public Answer answer { get; set; }
        //public object ticket { get; set; }
        //public DateTime serverDate { get; set; }
        //public string applicationProvider { get; set; }
        //public object metadata { get; set; }
        //public string _type { get; set; }
    }

}
