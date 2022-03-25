using System;
using System.Collections.Generic;

namespace OS_Sog.Models
{
    public class Answer
    {
        public string paymentOrderId { get; set; }
        public string paymentURL { get; set; }
        public string paymentOrderStatus { get; set; }
        public DateTime creationDate { get; set; }
        public object updateDate { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public string locale { get; set; }
        public string strongAuthentication { get; set; }
        public string orderId { get; set; }
        public ChannelDetails channelDetails { get; set; }
        public string paymentReceiptEmail { get; set; }
        public object taxRate { get; set; }
        public object taxAmount { get; set; }
        public DateTime expirationDate { get; set; }
        public TransactionDetails transactionDetails { get; set; }
        public bool dataCollectionForm { get; set; }
        public object merchantComment { get; set; }
        public string message { get; set; }
        public string _type { get; set; }
        public string formToken { get; set; }
    }

}
