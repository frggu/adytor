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
    /// <summary>
    /// Interaktionslogik für CopySelectChannelWindow.xaml
    /// </summary>
    public partial class CopySelectChannelWindow : Window
    {
        public CopySelectChannelWindow()
        {
            InitializeComponent();
        }

        private void ButtonClickCopy(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ButtonClickCancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
