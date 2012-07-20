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
using System.Windows.Shapes;
using TimelineEx;
using TimelineLibrary;

namespace EventGen
{
    /// <summary>
    /// Interaction logic for TimelineWnd.xaml
    /// </summary>
    public partial class TimelineWnd : Window
    {
        public TimelineWnd()
        {
            InitializeComponent();
        }
        
        private void timeline_TimelineReady(object sender, EventArgs e)
        {
            timeline.MinDateTime = DateTime.Now;
            timeline.MaxDateTime = timeline.MinDateTime.AddMinutes(3);
            timeline.CurrentDateTime = timeline.MinDateTime.AddMinutes(1); 
               
            var events = new List<TimelineEvent>();
            var rnd = new Random();
            for (var i = 0; i < 15; i++)
            {
                var te = new TimelineEvent();
                te.Description = string.Format("event{0} description",i);
                te.EventColor = "Green";
                te.IsDuration = false;
                te.Title = string.Format("event {0} title",i);
                te.StartDate = timeline.CurrentDateTime.AddMinutes(rnd.Next(5));
                te.EndDate = te.StartDate;
                events.Add(te);
            }

            timeline.ResetEvents(events);           
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(timeline.SelectedTimelineEvents.ElementAt(0).ToString());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var ev = timeline.SelectedTimelineEvents.First();           
            timeline.TimelineEvents.Remove(ev);
            timeline.ResetEvents(timeline.TimelineEvents);           
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            timeline.CurrentDateTime = timeline.CurrentDateTime.AddMinutes(2);
        }
    }
}
