using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Model
{
    public class Message
    {
        public MessageType Type { get; set; }
        public object? Content { get; set; }
        public Connection Connection { get; set; }
    }
}
