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

namespace SenitronPrintHandler
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : UserControl
    {
        public CustomMessageBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MessageProperty =
          DependencyProperty.Register("Message", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(""));


        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

     

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {

            Window.GetWindow(this).DialogResult = true;
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = false;
        }




    }
}
