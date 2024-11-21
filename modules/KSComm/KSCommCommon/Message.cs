namespace KSCommCommon
{
	[Serializable]
	public class Message
	{
		#region Properties
		public string Source { get; set; } = string.Empty;
		public string Destination { get; set; } = string.Empty;
		public byte[] Data { get; set; } = [];
		#endregion //Properties

		#region Constructor
		public Message()
		{
		}
		#endregion
		
		#region Public functions
		public byte[] ToBytes()
		{
			try
			{
				return Serializator.Serialize(this);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error serializing message: {ex.Message}");
				return null!;
			}
		}

		public static Message FromBytes(byte[] bytes)
		{
			try
			{
				return Serializator.Deserialize<Message>(bytes);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error deserializing message: {ex.Message}");
				return null!;
			}
		}
		#endregion //Public functions
	}
}
