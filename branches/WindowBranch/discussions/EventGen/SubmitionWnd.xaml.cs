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
using EventGen.timeline;
using LoginEngine;
using Reporter;
using Discussions;

namespace EventGen
{
    public partial class SubmitionWnd : Window
    {
        private Timeline _timelineModel;
        private DateTime _baseDateTime;
        private EventTotalsReport _totalsReport = new EventTotalsReport();

        public SubmitionWnd(Timeline timelineModel, DateTime baseDateTime)
        {
            InitializeComponent();

            _timelineModel = timelineModel;
            _baseDateTime = baseDateTime;

            //compute stats 
            var fakeEventId = 0;
            foreach (var te in _timelineModel.Events)
            {
                _totalsReport.CountEvent(te.e, fakeEventId++);
            }
            stats.Text = StatsUtils.GetEventTotals(_totalsReport).Text;
        }

        private void Submit_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var te in _timelineModel.Events)
            {
                DaoHelpers.recordEvent(te, _baseDateTime);
            }

            try
            {
                DbCtx.Get().SaveChanges();
            }
            catch (Exception e1)
            {
                MessageDlg.Show(e1.ToString(),
                                "Cannot submit events due to error: " + e1,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }

            Close();
        }
    }
}