using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Discussions.DbModel;
using Discussions.model;
using System.IO;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Input;
using System.Diagnostics;
using Discussions.rt;
using DistributedEditor;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for Badge.xaml
    /// </summary>
    public partial class Badge4 : UserControl
    {
        public delegate void OnToggleZoom();
        public OnToggleZoom onToggleZoom = null;

      ///  public BadgeWrapper clusterable = null;
      //  public IClusterManager clustMgr = null;

        MultiClickRecognizer mediaDoubleClick;

        public static readonly RoutedEvent RequestLargeViewEvent = EventManager.RegisterRoutedEvent(
         "RequestLargeView", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Badge4));

        public event RoutedEventHandler RequestLargeView
        {
            add { AddHandler(RequestLargeViewEvent, value); }
            remove { RemoveHandler(RequestLargeViewEvent, value); }
        }

        public void NotifyMoved()
        {
            //if (clustMgr == null)
            //    return;

            //clustMgr.NotifyLinkableMoved(clusterable);
            //clustMgr.NotifyClusterableMoved(clusterable);
        }

        public Badge4()
        {
            InitializeComponent();

            mediaDoubleClick = new MultiClickRecognizer(badgeDoubleTap,null);
        }

        public void HandleRecontext()
        {
            SetStyle();

            if (DataContext == null)
                Opacity = 0;
            else
                Opacity = 1;
        }

        void SetStyle()
        {            
            if (DataContext != null && DataContext is ArgPoint)
            {
                ArgPoint p = (ArgPoint)DataContext;
                mask.Background = new SolidColorBrush(Utils.IntToColor(p.Person.Color));
                switch ((SideCode)p.SideCode)
                {
                    case SideCode.Pros:
                        //lblSide.Content = "PROS";
                        //lblPerson.Background = DiscussionColors.prosBrush;
                        break;
                    case SideCode.Cons:
                        //lblSide.Content = "CONS";
                        //lblPerson.Background = DiscussionColors.consBrush;
                        break;
                    case SideCode.Neutral:
                        //lblSide.Content = "NEUTRAL";
                        //lblPerson.Background = DiscussionColors.neutralBrush;
                        break;
                    default:
                        throw new NotSupportedException();
                }
                //lblPerson.Content = p.Person.Name;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HandleRecontext();
        }

        public ArgPoint GetArgPoint()
        {
            ArgPoint p = (ArgPoint)DataContext;
            return p;
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            badgeDoubleTap(null,null);
        }

        private void root_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            mediaDoubleClick.Click(sender, e);
        }

        private void root_TouchDown_1(object sender, TouchEventArgs e)
        {
            mediaDoubleClick.Click(sender, e);
        }

        void badgeDoubleTap(object sender, InputEventArgs e)
        {
            //toggleZoom();
            //RaiseEvent(new RoutedEventArgs(RequestLargeViewEvent));

            var zoom = new ZoomWindow(DataContext as ArgPoint);
            zoom.ShowDialog();
        }

        public void SetCursorVisible(bool visible)
        {
            if(visible)
                usrCursor.Visibility = Visibility.Visible;
            else
                usrCursor.Visibility = Visibility.Hidden;
        }    
    }
}
