using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OnlineRadio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Mutex mutex;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool res;

            mutex = new Mutex(true, "Radio.exe", out res);
            if(!res)
            {
                MessageBox.Show("Radio is on");
                Environment.Exit(0);
            }    
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            mutex.ReleaseMutex();
        }
    }
}
