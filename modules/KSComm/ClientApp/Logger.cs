using System.IO;

namespace ClientApp
{
	public static class Logger
	{
		private const string FILE_PATH = "../../data/history";

		public static void Log(string message, string identification)
		{
			string newFilePath = FILE_PATH + identification + ".txt";
            if (!File.Exists(newFilePath)) File.Create(newFilePath).Close();
			File.AppendAllText(newFilePath, message);
		}

		public static string ReadLogs(string identification)
		{
            string newFilePath = FILE_PATH + identification + ".txt";
            if (!File.Exists(newFilePath)) File.Create(newFilePath).Close();
			return File.ReadAllText(newFilePath);
		}
	}
}
