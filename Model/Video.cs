using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Model
{
    public class Video
    {
        public string format {  get; set; }

        public List<byte[]> data { get; set; }
    }
}
