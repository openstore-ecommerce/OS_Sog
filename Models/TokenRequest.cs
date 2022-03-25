using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
