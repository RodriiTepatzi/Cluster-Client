using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core.Events
{
    public class ServerDisponibilityEventArgs : EventArgs
    {
        public string serverDisponibility { get; set; }

        public ServerDisponibilityEventArgs(string value)
        {
            serverDisponibility = value;
        }
    }
}
