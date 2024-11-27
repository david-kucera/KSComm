using System.Windows;
using KSCommClient;
using KSCommCommon;

namespace ClientApp
{
	public partial class MainWindow : Window
	{
		private string _identification = string.Empty;
		private Client _client = null!;
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Client_ClientMessageReceived(KSCommCommon.Message message)
		{
			var mess = message.Source + ": " + Serializator.Deserialize<string>(message.Data) + "\n";
			TextBoxMessages.Text += mess;
			Logger.Log(mess, _identification);
		}

		private void ButtonSendMessage_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(_identification) || _client == null) return;
			if (InputBoxMessages.Text.Length > 0)
			{
				var msg = new KSCommCommon.Message
				{
					Source = _identification,
					Data = Serializator.Serialize<string>(InputBoxMessages.Text)
				};
				_client.SendMessage(msg);
				InputBoxMessages.Text = "";
			}
		}

		private void Connect_Click(object sender, RoutedEventArgs e)
		{
            InputWindow inputWindow = new InputWindow();
			if (inputWindow.ShowDialog() == true) 
			{
                _identification = inputWindow.TextBoxId.Text;
                _client = new Client("http://localhost:5555/", _identification);
                _client.Open();
                _client.ClientMessageReceived += Client_ClientMessageReceived;
                TextBoxMessages.Text += Logger.ReadLogs(_identification);
            }
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Environment.Exit(0);
		}
    }
}