using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                if (e.Args[0] == "experiment")
                    SessionInfo.Get().ExperimentMode = true;
            }

            try
            {
                Directory.Delete(Utils.TempDir(), true);
            }
            catch(Exception)
            {
            }         
        }
    }
}