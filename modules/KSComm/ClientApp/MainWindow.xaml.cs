﻿using System.Windows;
using KSCommClient;
using KSCommCommon;

namespace ClientApp
{
	public partial class MainWindow : Window
	{
		private readonly string _identification = "Client1";
		private readonly Client _client;
		public MainWindow()
		{
			InitializeComponent();
			_client = new Client("http://localhost:5555/", _identification);
			_client.Open();
			_client.ClientMessageReceived += Client_ClientMessageReceived;
			TextBoxMessages.Text += Logger.ReadLogs();
		}

		private void Client_ClientMessageReceived(KSCommCommon.Message message)
		{
			var mess = message.Source + ": " + Serializator.Deserialize<string>(message.Data) + "\n";
			TextBoxMessages.Text += mess;
			Logger.Log(mess);
		}

		private void ButtonSendMessage_Click(object sender, RoutedEventArgs e)
		{
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
	}
}