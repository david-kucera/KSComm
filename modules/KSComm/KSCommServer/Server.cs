using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using KSCommCommon;

namespace KSCommServer
{
	#region Constants
	public enum State
	{
		Unknown,
		Opened,
		Closed
	}
	#endregion //Constants

	public class Server
	{
		#region Class members
		private readonly HttpListener _listener;
		private readonly string _url;
		private readonly ConcurrentDictionary<string, Client> _clients;
		#endregion //Class members

		#region Properties
		public State CurrentState = State.Unknown;
		#endregion //Properties

		#region Events
		public event Action<Message>? ServerMessageReceived = null;

		private void SendMessageReceived(Message message)
		{
			ServerMessageReceived?.Invoke(message);
		}
		#endregion

		#region Constructors
		public Server(string url)
		{
			_url = url;
			_listener = new HttpListener();
			_listener.Prefixes.Add(_url);
			_clients = new ConcurrentDictionary<string, Client>();
		}
		#endregion //Constructors

		#region Public functions
		public void Start()
		{
			_listener.Start();
			Console.WriteLine($"KSCommServer: Server started listening on {_url}.");
			CurrentState = State.Opened;
			ListenAsync();
		}

		public void Stop()
		{
			_listener.Stop();
			Console.WriteLine($"KSCommServer: Server stopped listening on {_url}.");
			CurrentState = State.Closed;
		}

		public void SendMessage(string clientId, Message message)
		{
			if (string.IsNullOrEmpty(message.Source))
			{
				message.Source = "Server";
			}
			byte[] bytes = message.ToBytes();

			try
			{
				if (_clients.TryGetValue(clientId, out Client? client))
				{
					client.WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);
				}
				else
				{
					Console.WriteLine($"WebSocketServer: Client {clientId} not found.");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"WebSocketServer: Error SendMessage: {e.Message}");
			}
		}
		#endregion //Public functions

		#region Private functions
		private async void ListenAsync()
		{
			while (CurrentState == State.Opened)
			{
				try
				{
					HttpListenerContext httpContext = await _listener.GetContextAsync();
					if (httpContext.Request.IsWebSocketRequest)
					{
						HttpListenerWebSocketContext wsContext = await httpContext.AcceptWebSocketAsync(null);
						Console.WriteLine($"KSCommServer: WebSocket connection established!");
						_ = Task.Run(() => HandleWebSocketConnection(wsContext.WebSocket));
					}
					else
					{
						httpContext.Response.StatusCode = 400;
						httpContext.Response.Close();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"KSCommServer: Error in ListenAsync: {ex.Message}");
				}
			}
		}

		private async Task HandleWebSocketConnection(WebSocket webSocket)
		{
			const int bufferSize = 1024 * 1024; // 1 MB
			ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[bufferSize]);
			string clientId = string.Empty;

			try
			{
				while (webSocket != null && webSocket.State != WebSocketState.Closed)
				{
					using var ms = new MemoryStream();
					WebSocketReceiveResult result;
					do
					{
						result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
						ms.Write(buffer.Array!, 0, result.Count);
					} while (!result.EndOfMessage);

					ms.Seek(0, SeekOrigin.Begin);
					var messageBytes = ms.ToArray();

					if (result.MessageType == WebSocketMessageType.Close)
					{
						await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing",
							CancellationToken.None);
					}
					else
					{
						var message = Message.FromBytes(messageBytes);
						clientId = message.Source;
						if (!_clients.ContainsKey(clientId))
						{
							var client = new Client(clientId, webSocket);
							_clients.TryAdd(clientId, client);
						}
						SendMessageReceived(message);
					}
				}
			}
			catch (WebSocketException wex)
			{
				Console.WriteLine($"KSCommServer: WebSocket error in HandleWebSocketConnection: {wex.Message}");
				_clients.Remove(clientId, out _);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"KSCommServer: General error in HandleWebSocketConnection: {ex.Message}");
				_clients.Remove(clientId, out _);
			}
		}
		#endregion //Private functions
	}
}
