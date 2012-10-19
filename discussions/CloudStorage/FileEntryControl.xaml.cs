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
using Discussions;

namespace CloudStorage
{
    public partial class FileEntryControl : UserControl
    {
        MultiClickRecognizer mediaDoubleClick;

        public static readonly RoutedEvent RequestViewEvent = EventManager.RegisterRoutedEvent(
                    "RequestView", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileEntryControl));

        public event RoutedEventHandler RequestView
        {
            add { AddHandler(RequestViewEvent, value); }
            remove { RemoveHandler(RequestViewEvent, value); }
        }

        public static readonly RoutedEvent CustSelectionEvent = EventManager.RegisterRoutedEvent(
                   "CustSelection", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileEntryControl));

        public event RoutedEventHandler CustSelection
        {
            add { AddHandler(CustSelectionEvent, value); }
            remove { RemoveHandler(CustSelectionEvent, value); }
        }


        DateTime recentClickStamp = DateTime.Now;
        
        public FileEntryControl()
        {
            InitializeComponent();

            mediaDoubleClick = new MultiClickRecognizer(badgeDoubleTap, null, onSingleClick);
        }

        void badgeDoubleTap(object sender, InputEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RequestViewEvent));
        }

        private void FileEntryControl_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {            
            mediaDoubleClick.Click(sender, e);
            e.Handled = true;
        }

        private void FileEntryControl_PreviewTouchDown_1(object sender, TouchEventArgs e)
        {
            mediaDoubleClick.Click(sender, e);
            e.Handled = true;
        }

        void onSingleClick(object sender, InputEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CustSelectionEvent));             
        }
    }
}
