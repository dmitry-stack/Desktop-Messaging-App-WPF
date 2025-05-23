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

namespace Wpf2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int index = 0;

        public MainWindow()
        {
            InitializeComponent();
        }
        public void AddMessage(string mes)
        {
            var bubble = new UserControl1(index, mes);
            Panel.Children.Add(bubble);
            index++;
        }
        public void SendMes(string text)
        {
            var bubble = new UserControl1(index, text)
            {
                Stamp = DateTime.Now
            };

            Panel.Children.Add(bubble);
            index++;
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Simple Chat Client");

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Enter(object sender, KeyEventArgs e)
        {
            string text = Input.Text.Trim();
            if (e.Key == Key.Enter)
            {
               
                SendMes(text);
                Input.Text = null;
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string text = Input.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                SendMes(text);
                Input.Text = null;
            }
        }
    }
}