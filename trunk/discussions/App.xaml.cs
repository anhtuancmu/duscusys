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
            if (e.Args.Length > 0 && e.Args[0] == "experiment")
            {
                SessionInfo.Get().ExperimentMode = true;
            }
            else if (e.Args.Length > 2)
            {
                SessionInfo.Get().ScreenshotMode = true;
                SessionInfo.Get().screenTopicId = int.Parse(e.Args[0]);
                SessionInfo.Get().screenDiscId = int.Parse(e.Args[1]);
                SessionInfo.Get().screenMetaInfo = e.Args[2];
            }

            //in screenshot mode, we may have temp images not yet built into report
            if (!SessionInfo.Get().ScreenshotMode)
            {
                try
                {
                    Directory.Delete(Utils.TempDir(), true);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}