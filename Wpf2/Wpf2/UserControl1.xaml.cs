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
        public int index;
        public DateTime Stamp = DateTime.Now;
        public string Date
        {
            get
            {
                var now= DateTime.Now;
                var diff= now - Stamp;
                //if (diff.TotalMinutes < 1)
                //{
                //    return "Now";

                //}
               return Stamp.ToString("dd/MM/yyyy");
            }

        }
        
        public UserControl1(int index,string text)
        {
            InitializeComponent();
            

            if (index % 2 == 0)
            {
                bubbleBorder.HorizontalAlignment = HorizontalAlignment.Left;
                usernameText.Text =Environment.UserName;
                date.Text = Date;
            }
            else
            {
                this.HorizontalAlignment = HorizontalAlignment.Right;
                usernameText.Text = "User";
                date.Text = Date;
            }

            messageContent.Text = text;

        }
    }
}
