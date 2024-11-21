using KSCommCommon;
using System.Net.WebSockets;

namespace KSCommClient
{
	public class Client
	{
		#region Class members
		private readonly Uri _serverUri;
		private ClientWebSocket? _webSocket = null;
		private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		#endregion //Class members

		#region Properties
		public WebSocket? WebSocket => _webSocket;
		public string Identification { get; set; }
		public bool IsConnected => _webSocket != null && _webSocket.State == WebSocketState.Open;
		#endregion //Properties

		#region Events
		public event Action<Message>? ClientMessageReceived = null;

		private void OnClientMessageReceived(Message message)
		{
			ClientMessageReceived?.Invoke(message);
		}
		#endregion //Events

		#region Constructor
		public Client(string serverUrl, string identification)
		{
			Identification = identification;
			_serverUri = new Uri(serverUrl.Replace("http", "ws"));
			_webSocket = new ClientWebSocket();
		}
		#endregion //Constructor

		#region Public functions

		public void Open()
		{
			if (IsConnected) return;

			_webSocket = new ClientWebSocket();
			_webSocket.ConnectAsync(_serverUri, CancellationToken.None).GetAwaiter().GetResult();
			_ = ReceiveMessagesAsync();
		}

		public void Close()
		{
			if (!IsConnected || _webSocket == null) return;

			try
			{
				_webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None).GetAwaiter().GetResult();
				_webSocket = null;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"KSCommClient: Error closing client: {ex.Message}");
			}
		}

		public void SendMessage(Message message)
		{
			if (!IsConnected || _webSocket == null) return;

			_semaphore.Wait();
			try
			{
				if (string.IsNullOrEmpty(message.Source)) message.Source = Identification;
				byte[] bytes = message.ToBytes();
				_webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"KSCommClient: Error sending message: {ex.Message}");
			}
			finally
			{
				_semaphore.Release();
			}
		}
		#endregion //Public functions

		#region Private functions
		private async Task ReceiveMessagesAsync()
		{
			int bufferSize = 1024 * 1024; // 1 MB
			var buffer = new ArraySegment<byte>(new byte[bufferSize]);
			try
			{
				while (_webSocket != null && _webSocket.State == WebSocketState.Open)
				{
					WebSocketReceiveResult result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);

					using var ms = new MemoryStream();
					ms.Write(buffer.Array!, buffer.Offset, result.Count);
					while (!result.EndOfMessage)
					{
						result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);
						ms.Write(buffer.Array!, buffer.Offset, result.Count);
					}
					ms.Seek(0, SeekOrigin.Begin);
					if (ms.Length == 0) continue;

					var message = Message.FromBytes(ms.ToArray());
					OnClientMessageReceived(message);
				}
			}
			catch (WebSocketException ex)
			{
				throw new WebSocketException($"KSCommClient: WebSocket error: {ex.Message}");
			}
			catch (Exception ex)
			{
				throw new Exception($"KSCommClient: General error: {ex.Message}");
			}
			finally
			{
				if (IsConnected)
				{
					Close();
				}
			}
		}
		#endregion //Private functions
	}
}
