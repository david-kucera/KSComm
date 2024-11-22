using System.IO;

namespace ClientApp
{
	public static class Logger
	{
		private const string FILE_PATH = "../../data/history.txt";

		public static void Log(string message)
		{
			if (!File.Exists(FILE_PATH)) File.Create(FILE_PATH).Close();
			File.AppendAllText(FILE_PATH, message);
		}

		public static string ReadLogs()
		{
			if (!File.Exists(FILE_PATH)) File.Create(FILE_PATH).Close();
			return File.ReadAllText(FILE_PATH);
		}
	}
}
