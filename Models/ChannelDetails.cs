using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Sog.Models
{
    public class ChannelDetails
    {
        public string channelType { get; set; }
        public object mailDetails { get; set; }
        public object smsDetails { get; set; }
        public object whatsAppDetails { get; set; }
        public string _type { get; set; }
    }


}
