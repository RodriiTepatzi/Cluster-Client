using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core.Events
{
    public class IsVideoReceivedEventArgs
    {
        public bool IsVideoReceived { get; set; }
        public string TemporalyPathVideoReceived {  get; set; }
        public IsVideoReceivedEventArgs (bool isVideoReceived, string temporalyPathVideoReceived)
        {
            IsVideoReceived = isVideoReceived;
            TemporalyPathVideoReceived = temporalyPathVideoReceived;
        }
    }
}
