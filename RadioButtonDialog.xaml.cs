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
using System.ComponentModel;
using System.ComponentModel;

namespace SenitronPrintHandler
{
    /// <summary>
    /// Interaction logic for RadioButtonDialog.xaml
    /// </summary>
    public partial class RadioButtonDialog : UserControl, INotifyPropertyChanged
    {
        public RadioButtonDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(RadioButtonDialog), new PropertyMetadata(""));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public bool GetRadioButtonIsChecked(string radioButtonName)
        {
            bool isChecked = false;
            foreach (RadioButton radioButton in this.radioButtonContainer.Children)
            {
                if (radioButtonName == radioButton.Name)
                {
                    if (radioButton.IsChecked == true)
                    {
                        isChecked = true;
                    }
                    break;
                }
            }
            return isChecked;
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
