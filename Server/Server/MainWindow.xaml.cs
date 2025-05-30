using System.Collections.Generic;
using System.IO;
using System.Net;
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

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener listener;
        public string password { get; set; }

        private Dictionary<TcpClient, string> clients = new();

        public MainWindow()
        {
            InitializeComponent();
            this.password = "1111";
        }

        public void Start(object sender, RoutedEventArgs e)
        {
            string address = txtAddress.Text;
            int port = int.Parse(txtPort.Text);
            listener = new TcpListener(IPAddress.Parse(address), port);
            listener.Start();
            Log($"Server started on {address}:{port}");
            this.password = txtPassword.Password.Trim();

            Thread acceptThread = new Thread(Accept) { IsBackground = true };
            acceptThread.Start();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Accept()
        {
            while (true)
            {
                try
                {
                    var client = listener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => Log($"Accept Error: {ex.Message}"));
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            using StreamReader reader = new(stream, Encoding.UTF8);
            using StreamWriter writer = new(stream, Encoding.UTF8) { AutoFlush = true };

            try
            {
                string authData = reader.ReadLine();
                if (string.IsNullOrEmpty(authData))
                {
                    writer.WriteLine("AUTH_FAIL");
                    client.Close();
                    Dispatcher.Invoke(() => Log("Authentication failed: no data received"));
                    return;
                }

                var parts = authData.Split('|');
                if (parts.Length < 2)
                {
                    writer.WriteLine("AUTH_FAIL");
                    client.Close();
                    Dispatcher.Invoke(() => Log("Authentication failed: incomplete credentials"));
                    return;
                }

                string username = parts[0].Trim();
                string providedPassword = parts[1].Trim();

                if (providedPassword != (this.password ?? "").Trim())
                {
                    writer.WriteLine("AUTH_FAIL");
                    client.Close();
                    Dispatcher.Invoke(() => Log($"Rejected: {username} (wrong password)"));
                    return;
                }

                writer.WriteLine("OK");

                clients[client] = username;
                Dispatcher.Invoke(() =>
                {
                    lstClients.Items.Add(username);
                    Log($"{username} connected");
                });

                BroadcastSystemMessage($"User {username} connected", client);

                string message;
                while ((message = reader.ReadLine()) != null)
                {
                    Dispatcher.Invoke(() => Log($"{username}: {message}"));
                    Broadcast($"{username}: {message}");
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Log($"Client error: {ex.Message}"));
            }
            finally
            {
                if (clients.TryGetValue(client, out var uname))
                {
                    Dispatcher.Invoke(() =>
                    {
                        lstClients.Items.Remove(uname);
                        Log($"{uname} disconnected");
                    });
                    BroadcastSystemMessage($"User {uname} disconnected", client);
                    clients.Remove(client);
                }
                client.Close();
            }
        }

        private void BroadcastSystemMessage(string message, TcpClient client)
        {
            Broadcast("[SYSTEM] " + message);
        }

        private void Log(string msg)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            string log = $"[{time}] {msg}";

            Dispatcher.Invoke(() =>
            {
                lstLog.Items.Add(log);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = txtServerMessage.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            Log($"[SERVER]: {message}");

            string fullMessage = $"[SERVER]: {message}";

            Broadcast(fullMessage);

            txtServerMessage.Clear();
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                Button_Click(sender, e);
            }
        }

        private void Broadcast(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");

            lock (clients)
            {
                foreach (var client in clients.Keys)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Log($"Error sending to {clients[client]}: {ex.Message}");
                    }
                }
            }

            Log($"Server: {message}");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string usernameToKick = lstClients.SelectedItem.ToString();

            var clientToKick = clients.FirstOrDefault(kvp => kvp.Value == usernameToKick).Key;
            if (clientToKick != null)
            {
                try
                {
                    NetworkStream stream = clientToKick.GetStream();
                    byte[] kickMsg = Encoding.UTF8.GetBytes("[SYSTEM] You have been kicked!\n");
                    stream.Write(kickMsg, 0, kickMsg.Length);
                }
                catch { }
                finally
                {
                    clientToKick.Close();
                    Log($"User {usernameToKick} was kicked.");
                }
            }
            else
            {
                Log("User not found or already disconnected.");
            }
        }
    }
}