using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core.Events
{
    public class VideoLoadedEventArgs : EventArgs
    {
        public bool IsVideoLoaded { get; set; }

        public VideoLoadedEventArgs(bool isVideoLoaded)
        {
            IsVideoLoaded = isVideoLoaded;
        }
    }
}
