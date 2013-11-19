using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Discussions.DbModel;
using Discussions.model;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for Badge.xaml
    /// </summary>
    public partial class Badge4 : UserControl
    {
        public delegate void OnToggleZoom();

        private MultiClickRecognizer mediaDoubleClick;

        public static readonly RoutedEvent RequestLargeViewEvent = EventManager.RegisterRoutedEvent(
            "RequestLargeView", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (Badge4));

        public event RoutedEventHandler RequestLargeView
        {
            add { AddHandler(RequestLargeViewEvent, value); }
            remove { RemoveHandler(RequestLargeViewEvent, value); }
        }

        public Badge4()
        {
            InitializeComponent();

            mediaDoubleClick = new MultiClickRecognizer(badgeDoubleTap, null);
        }

        void ToggleDot(bool notificationsExist)
        {
            if (notificationsExist)
                notDot.Visibility = Visibility.Visible;
            else
                notDot.Visibility = Visibility.Collapsed;
        }

        public void HandleRecontext()
        {
            SetStyle();

            if (DataContext == null)
                Opacity = 0;
            else
                Opacity = 1;
        }

        private void SetStyle()
        {
            if (DataContext != null && DataContext is ArgPoint)
            {
                ArgPoint p = (ArgPoint) DataContext;
                if (p.Person == null)
                    return;

                var numUnread = DaoUtils.NumCommentsUnreadBy(new DiscCtx(ConfigManager.ConnStr), p.Id).Total();
                ToggleDot(numUnread>0);

                mask.Background = new SolidColorBrush(Utils.IntToColor(p.Person.Color));
                switch ((SideCode) p.SideCode)
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
            ArgPoint p = (ArgPoint) DataContext;
            return p;
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            badgeDoubleTap(null, null);
        }

        private void root_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            mediaDoubleClick.Click(sender, e);
        }

        private void root_TouchDown_1(object sender, TouchEventArgs e)
        {
            mediaDoubleClick.Click(sender, e);
        }

        private void badgeDoubleTap(object sender, InputEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RequestLargeViewEvent));
            if(e!=null)
                e.Handled = true;
            //var ap = DataContext as ArgPoint;
            //var id = ap.Id;
            //var zoomedAp = DbCtx.Get().ArgPoint.FirstOrDefault(ap0 => ap0.Id == id);

            //var zoom = new ZoomWindow(zoomedAp);
            //zoom.ShowDialog();
        }

        public void SetCursorVisible(bool visible)
        {
            if (visible)
                usrCursor.Visibility = Visibility.Visible;
            else
                usrCursor.Visibility = Visibility.Hidden;
        }
    }
}