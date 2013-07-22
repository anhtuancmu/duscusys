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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EventGen
{
    /// <summary>
    /// Interaction logic for EventButton.xaml
    /// </summary>
    public partial class EventButton : UserControl
    {
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "ClickEvent", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (EventButton));

        // Provide CLR accessors for the event
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public EventButton()
        {
            InitializeComponent();
        }

        public string EventName
        {
            get { return eventName.Text; }
            set { eventName.Text = value; }
        }

        public string EventCount
        {
            get { return eventCount.Text; }
            set { eventCount.Text = value; }
        }

        private void EventButton_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            Background = null;
            RaiseEvent(new RoutedEventArgs(ClickEvent));
            e.Handled = true;
        }

        private void EventButton_TouchUp_1(object sender, TouchEventArgs e)
        {
            Background = null;
            RaiseEvent(new RoutedEventArgs(ClickEvent));
            e.Handled = true;
        }

        private void EventButton_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Background = Brushes.LightGray;
            e.Handled = true;
        }

        private void EventButton_PreviewTouchDown_1(object sender, TouchEventArgs e)
        {
            Background = Brushes.LightGray;
            e.Handled = true;
        }

        private void EventButton_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Background = null;
        }
    }
}