using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core.Events
{
    public class IsVideoSendedEventArgs : EventArgs
    {
        public bool IsVideoSended { get; set; }

        public IsVideoSendedEventArgs(bool isVideoSended)
        {
            IsVideoSended = isVideoSended;
        }
    }
}
