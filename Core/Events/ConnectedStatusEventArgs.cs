using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core.Events
{
	public class ConnectedStatusEventArgs : EventArgs
	{
		public bool Connected { get; set; }

        public ConnectedStatusEventArgs(bool isConnected)
        {
            Connected = isConnected;
        }
    }
}
