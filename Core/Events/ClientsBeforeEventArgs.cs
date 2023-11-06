using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core.Events
{
    public class ClientsBeforeEventArgs : EventArgs
    {
        public int clientsBefore {  get; set; }
        public ClientsBeforeEventArgs(int value) 
        {
            clientsBefore = value;
        }
    }
}
