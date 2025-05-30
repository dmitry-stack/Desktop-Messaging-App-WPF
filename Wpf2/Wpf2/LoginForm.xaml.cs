using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace Wpf2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        public LoginForm()
        {
            InitializeComponent();
        }
        public TcpClient Client { get;  set; }
        public string Username { get;  set; }


        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            
            btnSubmit.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;

            string username = txtUsername.Text;
            string password = txtPassword.Password;
            string address = txtAddress.Text;

            if (!int.TryParse(txtPort.Text, out int port))
            {
                MessageBox.Show("Invalid port number");
                ResetUI();
                return;
            }

            try
            {
                Client = new TcpClient();
                await Client.ConnectAsync(address, port);
                NetworkStream stream = Client.GetStream();

                string credentials = $"{username}|{password}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(credentials);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                byte[] responseBuffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                if (response.StartsWith("OK"))
                {
                    MessageBox.Show("Connection successful!");
                    Username = username;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Authentication failed: " + response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
            finally
            {
                ResetUI();
            }
        }

        private void ResetUI()
        {
            btnSubmit.IsEnabled = true;
            progressBar.Visibility = Visibility.Collapsed;
        }
    }
}
