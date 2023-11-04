using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core.Events
{
	public class MessageReceivedEventArgs : EventArgs
	{
		public string Message { get; set; }

		public MessageReceivedEventArgs(string value) 
		{
			Message = value;
		}
	}
}
