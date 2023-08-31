using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SenitronPrintHandler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        protected override void OnExit(ExitEventArgs e)
        {

            // when app existed 

            MessageBox.Show("app is closing !!!!!");

            base.OnExit(e);
        }

    }
}
