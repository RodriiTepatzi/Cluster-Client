using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core.Events
{
    public class LocalVideoEventArgs
    {
        public string LocalVideoPath { get; set; }

        public LocalVideoEventArgs(string localVideoPath)
        {
            LocalVideoPath = localVideoPath;
        }
    }
}
