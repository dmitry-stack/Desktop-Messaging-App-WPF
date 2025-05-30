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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public HorizontalAlignment Alignment { get; set; }
        public UserControl1()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                UpdateVisuals();
            };
        }


        private void UpdateVisuals()
        {
            if (DataContext is MessageModel msg)
            {
                bubbleBorder.HorizontalAlignment = msg.Alignment;
                usernameText.Text = msg.Username;
                messageContent.Text = msg.Message;
                date.Text = GetFormattedDate(msg.Timestamp);
                date.ToolTip = msg.Timestamp.ToString("dd MMM yyyy HH:mm:ss");
            }
        }

        public void RefreshTimestamp()
        {
            if (DataContext is MessageModel msg)
            {
                date.Text = GetFormattedDate(msg.Timestamp);
                date.ToolTip = msg.Timestamp.ToString("dd MMM yyyy HH:mm:ss");
            }
        }

        private string GetFormattedDate(DateTime stamp)
        {
            var now = DateTime.Now;
            var diff = now - stamp;

            if (diff.TotalMinutes < 1)
                return "Now";
            if (diff.TotalMinutes < 15)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return stamp.ToString("HH:mm");
            return stamp.ToString("dd/MM/yyyy");
        }
    }
}
