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
        TreeViewItem topicSection;
        TreeViewItem usersSection;

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

            topicSection = ((TreeViewItem)reportRoot.Items[0]);
            usersSection = ((TreeViewItem)reportRoot.Items[1]);
        }

        public void onJoin()
        {
            UISharedRTClient.Instance.clienRt.onJoin -= onJoin;

            var discId = 1;
            _collector = new ReportCollector(discId, null, null, reportGenerated);
        }

        TextBlock GetTopicSummary(TopicReport report)
        {            
            var txt = "  Number of clustered badges: " + report.numClusteredBadges + "\n";
            txt += "  Number of clusters: " + report.numClusters + "\n";
            txt += "  Number of links: " + report.numLinks + "\n";
            txt += "  Number of users: " + report.numParticipants + "\n";
            txt += "  Number of sources: " + report.numSources + "\n";
            txt += "  Number of comments: " + report.numComments + "\n";
            txt += "  Cumulative duration: " + TimeSpan.FromSeconds(report.cumulativeDuration) + "\n";

            var res = new TextBlock();
            res.Text = txt;
            return res;
        }

        TextBlock GetCluster(ClusterReport report)
        {
            var txt = "Cluster " + report.clusterTitle + "\n";            
            foreach (var ap in report.points)
                txt  += "  arg. point:  " + ap.Point + "\n";
            
            var res = new TextBlock();
            res.Text = txt;
            return res;
        }

        TextBlock GetLink(LinkReportResponse report)
        {
            var txt = "Link \n";
            if (report.EndpointArgPoint1)
                txt += "  endpoint 1 is arg. point: " + report.ArgPoint1.Point + "\n";
            else
                txt += "  endpoint 1 is cluster: " + report.ClusterCaption1 + "\n";

            if (report.EndpointArgPoint2)
                txt += "  endpoint 2 is arg. point: " + report.ArgPoint2.Point + "\n";
            else
                txt += "  endpoint 2 is cluster: " + report.ClusterCaption2 + "\n";            

            var res = new TextBlock();
            res.Text = txt;
            return res;
        }

        TextBlock GetTextBlock(string txt)
        {
            var res = new TextBlock();
            res.Text = txt;
            return res;        
        }

        TreeViewItem GetTopicReport(TopicReport report)
        {            
            var res = new TreeViewItem();
            res.Items.Add(GetTopicSummary(report));
            res.Header = report.topic.Name;

            //clusters
            bool hasClusters = false;
            foreach (var clustReport in _collector.ClusterReports)
            {
                if (clustReport.topic.Id != report.topic.Id)
                    continue;

                res.Items.Add(GetCluster(clustReport));
                hasClusters = true;
            }
            if (!hasClusters)
                 res.Items.Add(GetTextBlock("<no clusters in topic>"));

            //links
            bool hasLinks = false;
            foreach (var linkReport in _collector.LinkReports)
            {
                if (linkReport.topicId != report.topic.Id)
                    continue;

                res.Items.Add(GetLink(linkReport));             
                hasLinks = true;
            }
            if (!hasLinks)
                res.Items.Add(GetTextBlock("<no links in topic>"));


            return res;
        }

        TreeViewItem GetTotalTopicSummary(TopicReport report)
        {
            var res = new TreeViewItem();
            res.Header = "<all topics total>";            
            res.Items.Add(GetTopicSummary(report));
            return res;
        }

        TreeViewItem GetUserSummary(ArgPointReport report, bool allUsers)
        {
            var txt = "  Number of comments " + report.numComments + "\n";
            txt += "  Number of media attachments " + report.numMediaAttachments + "\n";
            txt += "  Number of points " + report.numPoints + "\n";
            txt += "  Number of points with description " + report.numPointsWithDescriptions + "\n";
            txt += "  Number of sources " + report.numSources + "\n";                        

            var tb = new TextBlock();
            tb.Text = txt;
            var tvi = new TreeViewItem();
            tvi.Items.Add(tb);
            if (allUsers)
                tvi.Header = "<all users total>";
            else
                tvi.Header = "User " + report.user.Name;

            return tvi;
        }

        TreeViewItem GetAvgUserSummary(ArgArgPointReport report)
        {
            var txt = "  Number of comments " + report.numComments + "\n";
            txt += "  Number of media attachments " + report.numMediaAttachments + "\n";
            txt += "  Number of points " + report.numPoints + "\n";
            txt += "  Number of points with description " + report.numPointsWithDescriptions + "\n";
            txt += "  Number of sources " + report.numSources + "\n";

            var tb = new TextBlock();
            tb.Text = txt;
            var tvi = new TreeViewItem();
            tvi.Items.Add(tb);
            tvi.Header = "<average user>";
            return tvi;
        }

        public void reportGenerated()
        {
            topicSection.Items.Clear();
            foreach (var topicReport in _collector.TopicReports)
                topicSection.Items.Add(GetTopicReport(topicReport));

            var totals = _collector.AllTopicsReport;
            topicSection.Items.Add(GetTotalTopicSummary(totals));



            usersSection.Items.Clear();
            foreach(var report in _collector.ArgPointReports.Values)
                usersSection.Items.Add(GetUserSummary(report, false));

            usersSection.Items.Add(GetUserSummary(_collector.TotalArgPointReport, true));

            usersSection.Items.Add(GetAvgUserSummary(_collector.AvgArgPointReport));
        }

        LoginResult testLoginStub(DiscCtx ctx)
        {
            var loginRes = new LoginResult();
            loginRes.discussion = ctx.Discussion.First();
            loginRes.person     = ctx.Person.FirstOrDefault(p0 => p0.Name.StartsWith("moder"));
            return loginRes;
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            Console.Beep();
        }

        private void btnRun_Click_1(object sender, RoutedEventArgs e)
        {
            var discId = 1;
            _collector = new ReportCollector(discId, null, null, reportGenerated);
        }
    }
}
