using KSCommCommon;
using KSCommServer;

namespace ServerApp
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Server starting...");
			Server server = new Server("http://localhost:5555/");
			server.Start();
			server.ServerMessageReceived += (message) =>
			{
				Console.WriteLine($"Server received message: {message}");
				var replyMessage = new Message
				{
					Source = "Server",
					Data = Serializator.Serialize("[" + DateTime.Now + "]: " +  Serializator.Deserialize<string>(message.Data))
				};
				server.SendMessage(message.Source, replyMessage);
			};
			while (true)
			{
				if (Console.ReadLine() == "exit")
				{
					break;
				}
			}

			Console.WriteLine("Server stopping...");
			server.Stop();
			Console.WriteLine("Server stopped");
		}
	}
}
