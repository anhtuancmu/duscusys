using System.Windows;
using Discussions.view;

namespace Discussions
{
    public class DiscWindows
    {
        private static DiscWindows _inst = null;

        public Main mainWnd = null;
        public PublicCenter discDashboard = null;
        public PrivateCenter3 privateDiscBoard = null;
        public Dashboard moderDashboard = null;
        public ResultViewer resViewer = null;
        public PersonManagerWnd persMgr = null;
        public HtmlEditWnd htmlBackgroundWnd = null;

        public static DiscWindows Get()
        {
            if (_inst == null)
                _inst = new DiscWindows();

            return _inst;
        }

        public void CloseUserDashboards()
        {
            if (privateDiscBoard == null && discDashboard == null)
                return;

            if (privateDiscBoard != null)
            {
                privateDiscBoard.Close();
                privateDiscBoard = null;
            }

            if (discDashboard != null)
            {
                discDashboard.Close();
                discDashboard = null;
            }

            if (htmlBackgroundWnd != null)
            {
                htmlBackgroundWnd.Close();
                htmlBackgroundWnd = null;
            }

            MessageDlg.Show("Moderator is offline or selected session is not running. User dashboards have been closed",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        public void CloseAndDispose()
        {
            mainWnd = null;

            if (discDashboard != null)
            {
                discDashboard.Close();
                discDashboard = null; //todo
            }

            if (moderDashboard != null)
            {
                moderDashboard.Close();
                moderDashboard = null;
            }

            if (resViewer != null)
            {
                resViewer.Close();
                resViewer = null;
            }

            if (privateDiscBoard != null)
            {
                privateDiscBoard.Close();
                privateDiscBoard = null;
            }

            if (persMgr != null)
            {
                persMgr.Close();
                persMgr = null;
            }

            if (htmlBackgroundWnd != null)
            {
                htmlBackgroundWnd.Close();
                htmlBackgroundWnd = null;
            }
        }

        public void HideOwnWindows()
        {
            DiscWindows.Get().mainWnd.Hide();
            if (DiscWindows.Get().privateDiscBoard != null)
                DiscWindows.Get().privateDiscBoard.Hide();
            if (DiscWindows.Get().moderDashboard != null)
                DiscWindows.Get().moderDashboard.Hide();
            if (DiscWindows.Get().htmlBackgroundWnd != null)
                DiscWindows.Get().htmlBackgroundWnd.Hide();
            if (DiscWindows.Get().discDashboard != null)
                DiscWindows.Get().discDashboard.Hide();
        }

        public void ShowOwnWindows()
        {
            DiscWindows.Get().mainWnd.Show();
            if (DiscWindows.Get().privateDiscBoard != null)
                DiscWindows.Get().privateDiscBoard.Show();
            if (DiscWindows.Get().moderDashboard != null)
                DiscWindows.Get().moderDashboard.Show();
            if (DiscWindows.Get().htmlBackgroundWnd != null)
                DiscWindows.Get().htmlBackgroundWnd.Show();
            if (DiscWindows.Get().discDashboard != null)
                DiscWindows.Get().discDashboard.Show();
        }
    }
}