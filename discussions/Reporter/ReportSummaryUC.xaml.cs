using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Reporter
{ 
    public partial class ReportSummaryUC : UserControl
    {
        ReportParameters _parameters = null;

        public ReportSummaryUC()
        {
            InitializeComponent();
        }

        public delegate void OnReportParamsChanged(ReportParameters parameters);
        public OnReportParamsChanged ParamsChanged = null;
      
        public ReportParameters getReportParams(bool forceDlg)
        {
            if (_parameters != null && !forceDlg)
                return _parameters;

            var dlg = new SessionTopicDlg();
            dlg.ShowDialog();

            if (dlg.reportParameters != null)
            {
                Session = dlg.reportParameters.session.Name;
                Topic   = dlg.reportParameters.topic.Name;               
            }
            else
            {
                Session = "";
                Topic = "";
            }

            _parameters = dlg.reportParameters;

            return _parameters;
        }

        public string Topic
        {
            set
            {
                topicName.Text = value;
            }
        }

        public string Session
        {
            set
            {
                sessionName.Text = value;
            }
        }

        public void SetParticipants(ObservableCollection<Person> persons)
        {
            participants.ItemsSource = persons;
        }

        private void btnSessionTopic_Click_1(object sender, RoutedEventArgs e)
        {
            getReportParams(true);

            if (ParamsChanged != null)
                ParamsChanged(_parameters); 
        }
    }
}
