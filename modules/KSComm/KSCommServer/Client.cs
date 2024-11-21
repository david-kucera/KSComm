using System.Net.WebSockets;

namespace KSCommServer
{
	public class Client
	{
		#region Properties
		public string Identification { get; set; }
		public WebSocket WebSocket { get; set; }
		#endregion // Properties

		#region Constructors
		public Client(string identification, WebSocket webSocket)
		{
			Identification = identification;
			WebSocket = webSocket;
		}
		#endregion // Constructors
	}
}
