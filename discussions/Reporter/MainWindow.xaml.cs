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
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace Reporter
{   
    public partial class MainWindow : Window
    {
        ReportCollector _collector; 

        public MainWindow()
        {
            InitializeComponent();

            var discCtx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            var loginInfo = testLoginStub(discCtx);

            if (loginInfo == null)
            {
                Application.Current.Shutdown();
                return;
            }
         
            UISharedRTClient.Instance.start(loginInfo,
                                            discCtx.Connection.DataSource, 
                                            DeviceType.Wpf);

            UISharedRTClient.Instance.clienRt.onJoin += onJoin;
        }

        public void onJoin()
        {
            UISharedRTClient.Instance.clienRt.onJoin -= onJoin;

            var discId = 1;
            _collector = new ReportCollector(discId, TopicReportReady, AllTopicTotals, reportGenerated);
        }

        public void reportGenerated()
        {
            log.Text += "\n CLUSTERS \n";
            bool hasClusters = false; 
            foreach(var clustReport in _collector.ClusterReports)
            {
                log.Text += " title=" + clustReport.clusterTitle + ":\n";
                log.Text += " cluster topic " + clustReport.topic.Name + ":\n";
                log.Text += "{\n";
                foreach(var ap in clustReport.points)
                    log.Text += ap.Point + " \n";
                log.Text += "}\n";
                hasClusters = true;
            }
            if (!hasClusters)
            {
                log.Text += "\n no cluster \n"; 
            }
        }

        LoginResult testLoginStub(DiscCtx ctx)
        {
            var loginRes = new LoginResult();
            loginRes.discussion = ctx.Discussion.FirstOrDefault(d0 => d0.Subject.StartsWith("d-editor"));
            loginRes.person     = ctx.Person.FirstOrDefault(p0 => p0.Name.StartsWith("moder"));
            return loginRes;
        }

        public void AllTopicTotals(TopicReport report)
        {
            log.Text += "ALL TOPICS TOTAL: " + " NumClusteredBadges " + report.numClusteredBadges +
                        " NumClusters " + report.numClusters + " NumLinks " + report.numLinks + 
                        " users " + report.numParticipants  + " num sources " + report.numSources + 
                        " comments " + report.numComments + 
                        " cumulative duration " + TimeSpan.FromSeconds(report.cumulativeDuration) + "\n"; 
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            Console.Beep();
        }

        public void TopicReportReady(TopicReport report)
        {
            log.Text += "Topic " + report.topic.Name + ": NumClusteredBadges " + report.numClusteredBadges +
                        " NumClusters " + report.numClusters + " NumLinks " + report.numLinks +
                        " users " + report.numParticipants + " num sources " + report.numSources +
                        " comments " + report.numComments +
                        " cumulative duration " + TimeSpan.FromSeconds(report.cumulativeDuration) + "\n"; 
        }

        private void btnRun_Click_1(object sender, RoutedEventArgs e)
        {
            log.Text = "";
            var discId = 1;
            _collector = new ReportCollector(discId, TopicReportReady, AllTopicTotals, reportGenerated);
        }
    }
}
