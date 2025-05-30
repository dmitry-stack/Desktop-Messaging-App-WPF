using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Wpf2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int messageIndex = 0;
        DispatcherTimer timer = new DispatcherTimer();

        bool isConnected = false;
        TcpClient client;
        CancellationTokenSource cancellationTokenSource;
        public string cur_username {get;set;}

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += (s, e) => RefreshTimestamps();
            timer.Start();
        }

        private void SendMessage()
        {
            string text = Input.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            if (client != null && client.Connected)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(text + "\n");
                    client.GetStream().Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    AddSystemMessage("Send error: " + ex.Message);
                }
            }
            else
            {
                DisplayMessage(Environment.UserName, text);
            }
            Input.Clear();
        }

        private void DisplayMessage(string username, string message)
        {
            
            HorizontalAlignment align = HorizontalAlignment.Left;
            if (username.Equals(cur_username, StringComparison.OrdinalIgnoreCase))
            {
                align = HorizontalAlignment.Right;
            }
            if (username.StartsWith("[SYSTEM]") || username.StartsWith("[SERVER]"))
            {
                align = HorizontalAlignment.Center;
            }

            var msg = new MessageModel
            {
                Username = username,
                Message = message,
                Timestamp = DateTime.Now,
                Alignment = align
            };

            var bubble = new UserControl1 { DataContext = msg };
            Panel.Children.Add(bubble);
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            LoginForm login = new LoginForm();
            if (login.ShowDialog() == true)
            {
                TcpClient connectedClient = login.Client;
                if (connectedClient != null && connectedClient.Connected)
                {
                    client = connectedClient; 
                    isConnected = true;
                    ConnectMenuItem.IsEnabled = false;
                    DisconnectMenuItem.IsEnabled = true;
                    cur_username = login.Username;
                    AddSystemMessage("Connected");
                    StartListeningForMessages();
                }
                else
                {
                    AddSystemMessage("Failed to connect to the server");
                }
            }
        }

        private async void StartListeningForMessages()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            try
            {
                NetworkStream stream = client.GetStream();
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    while (!token.IsCancellationRequested)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line == null) break;
                        if (line.Contains("You have been kicked"))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                AddSystemMessage(line);
                                MessageBox.Show("You have been kicked by the server.");
                                isConnected = false;
                                cancellationTokenSource?.Cancel();
                                client?.Close();
                                ConnectMenuItem.IsEnabled = true; 
                                DisconnectMenuItem.IsEnabled = false; 
                                AddSystemMessage("Disconnected");
                            });
                            break;
                        }

                        string username = "Unknown";
                        string messageText = line;
                        int colonIndex = line.IndexOf(':');
                        if (colonIndex > 0 && !line.StartsWith("["))
                        {
                            username = line.Substring(0, colonIndex).Trim();
                            messageText = line.Substring(colonIndex + 1).Trim();
                        }
                        else if (line.StartsWith("[SYSTEM]") || line.StartsWith("[SERVER]"))
                        {
                            username = line.Substring(0, line.IndexOf(' '));
                            messageText = line;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            DisplayMessage(username, messageText);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    AddSystemMessage("Error receiving messages: " + ex.Message);
                });
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected)
            {
                isConnected = false;
                cancellationTokenSource?.Cancel();
                client?.Close();
                ConnectMenuItem.IsEnabled = true;
                DisconnectMenuItem.IsEnabled = false;
                AddSystemMessage("Disconnected");
            }
        }

        public void RefreshTimestamps()
        {
            foreach (var child in Panel.Children)
            {
                if (child is UserControl1 bubble && bubble.DataContext is MessageModel msg)
                {
                    msg.Timestamp = msg.Timestamp;
                    bubble.DataContext = null;
                    bubble.DataContext = msg;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Simple Chat Client");
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void AddSystemMessage(string text)
        {
            var msg = new TextBlock
            {
                Text = text,
                Foreground = Brushes.DarkGray,
                FontStyle = FontStyles.Italic,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5)
            };
            Panel.Children.Add(msg);
        }

        public void Input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = true;
                SendMessage();
            }
        }
    }
}