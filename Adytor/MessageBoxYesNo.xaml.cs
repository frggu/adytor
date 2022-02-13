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

namespace Adytor
{
    public partial class MessageBoxYesNo : Window
    {
        public MessageBoxYesNo()
        {
            InitializeComponent();
        }

        public MessageBoxYesNo(string Message, string title)
        {
            InitializeComponent();
            this.lblMessage.Text = Message;
            this.Title = title;
        }

        private void ButtonClickYes(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ButtonClickNo(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
