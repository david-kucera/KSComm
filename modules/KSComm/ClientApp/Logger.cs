using System.IO;

namespace ClientApp
{
	public static class Logger
	{
		private const string FILE_PATH = "../../data";

		public static void Log(string message, string identification)
		{
			if (!Directory.Exists(FILE_PATH)) Directory.CreateDirectory(FILE_PATH);
            string newFilePath = FILE_PATH + identification + ".txt";
            if (!File.Exists(newFilePath)) File.Create(newFilePath).Close();
			File.AppendAllText(newFilePath, message);
		}

		public static string ReadLogs(string identification)
		{
            if (!Directory.Exists(FILE_PATH)) Directory.CreateDirectory(FILE_PATH);
            string newFilePath = FILE_PATH + identification + ".txt";
            if (!File.Exists(newFilePath)) File.Create(newFilePath).Close();
			return File.ReadAllText(newFilePath);
		}
	}
}
