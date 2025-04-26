using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
    public partial class MainWindow : Window
    {
        const string serverAddress = "127.0.0.1";
        const int port = 4040;
        IPEndPoint server;
        UdpClient client;
        ObservableCollection<MessageInfo> messages;
        private string nickname = "";
        private bool isListening = false;

        public MainWindow()
        {
            InitializeComponent();
            server = new IPEndPoint(IPAddress.Parse(serverAddress), port);
            client = new UdpClient();
            messages = new ObservableCollection<MessageInfo>();
            this.DataContext = messages;
        }

        private void SendBtn(object sender, RoutedEventArgs e)
        {
            string message = msgTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Enter message", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                MessageBox.Show("First enter a nickname", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SendMessage($"{nickname}: {message}");
            msgTextBox.Text = "";
        }

        private void msgTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendBtn(sender, e);
            }
        }

        private void JoinBtn(object sender, RoutedEventArgs e)
        {
            nickname = nicknameBox.Text.Trim();
            if (string.IsNullOrEmpty(nickname))
            {
                MessageBox.Show("Enter nickname", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SendMessage("$<join>");
            if (!isListening)
            {
                isListening = true;
                Listner();
            }
        }

        private async void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            await client.SendAsync(data, data.Length, server);
        }

        private async void Listner()
        {
            while (isListening)
            {
                try
                {
                    var data = await client.ReceiveAsync();
                    string message = Encoding.Unicode.GetString(data.Buffer);
                    Dispatcher.Invoke(() => messages.Add(new MessageInfo(message)));
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private void LeaveBtn(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(nickname))
            {
                SendMessage($"$<leave> {nickname}");
                messages.Add(new MessageInfo($"** {nickname} left the chat **"));
            }

            isListening = false;
            nickname = "";
            client.Close();
        }

        private void ClearBtn(object sender, RoutedEventArgs e)
        {
            messages.Clear();
        }
    }

    public class MessageInfo
    {
        public string Message { get; set; }
        private DateTime time;
        public string Time => time.ToLongTimeString();

        public MessageInfo(string message)
        {
            Message = message;
            time = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Message} ({Time})";
        }
    }
}
