using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core.Events
{
    public class LocalVideoEventArgs
    {
        public byte[] LocalVideo { get; set; }

        public LocalVideoEventArgs(byte[] localVideo)
        {
            LocalVideo = localVideo;
        }
    }
}
