using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            var txt = "  Cumulative duration: " + TimeSpan.FromSeconds(report.cumulativeDuration) + "\n";
            txt += "  Number of users: " + report.numParticipants + "\n";
            txt += "  Number of arg. points: " + report.numPoints + "\n";
            txt += "  Number of arg. points with description: " + report.numPointsWithDescription + "\n";
            txt += "  Number of media attachments: " + report.numMediaAttachments + "\n";
            txt += "  Number of sources: " + report.numSources + "\n";
            txt += "  Number of comments: " + report.numComments + "\n";
            txt += "  Number of clustered badges: " + report.numClusteredBadges + "\n";
            txt += "  Number of clusters: " + report.numClusters + "\n";
            txt += "  Number of links: " + report.numLinks;
          
            var res = new TextBlock();
            res.Text = txt;
            return res;
        }

        TreeViewItem GetCluster(ClusterReport report)
        {
            var res = new TreeViewItem();
            res.Header = "Cluster " + report.clusterTitle;

            foreach (var ap in report.points)
                res.Items.Add(GetPointReport(ap));

            return res;
        }

        TreeViewItem GetLink(LinkReportResponse report)
        {
            var res = new TreeViewItem();
            res.Header = "Link";

            if (report.EndpointArgPoint1)
                res.Items.Add(GetPointReport(report.ArgPoint1));
            else
                res.Items.Add(new TextBlock(new Run("Cluster " + report.ClusterCaption1)));

            if (report.EndpointArgPoint2)
                res.Items.Add(GetPointReport(report.ArgPoint2));
            else
                res.Items.Add(new TextBlock(new Run("Cluster " + report.ClusterCaption2)));    

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

        TreeViewItem GetPointReport(ArgPoint ap)
        {
            var txt = new TextBlock();
            if (ap.Description.Text != "Description")
                txt.Text += ap.Description.Text;
            else
                txt.Text += "No description";

            var res = new TreeViewItem();
            res.Header = "Arg. point " + ap.Point;
            res.Items.Add(txt);
             
            var usr = new MiniUserUC();
            usr.DataContext = ap.Person;
            res.Items.Add(usr);

            return res;
        }

        TreeViewItem GetUserOneTopicSummary(ArgPointReport report, bool totalUser)
        {
            var txt = "  Number of points " + report.numPoints + "\n";
            txt += "  Number of points with description " + report.numPointsWithDescriptions + "\n";
            txt += "  Number of media attachments " + report.numMediaAttachments + "\n";
            txt += "  Number of sources " + report.numSources + "\n";   
            txt += "  Number of comments " + report.numComments;
           
            var tb = new TextBlock();
            tb.Text = txt;
            var tvi = new TreeViewItem();
            tvi.Items.Add(tb);

            if (!totalUser)
            {
                var usrId = report.user.Id;
                foreach (var ap in report.topic.ArgPoint.Where(ap0=>ap0.Person.Id == usrId))
                    tvi.Items.Add(GetPointReport(ap));
            }

            if (report.topic != null && !totalUser)
                tvi.Header = report.topic.Name;
            else
                tvi.Header = "<total user, all topics>"; 

            return tvi;
        }

        TreeViewItem GetUserSummary(List<ArgPointReport> reportsOfUser)
        {
            var res = new TreeViewItem();
            string usrName = "";
            foreach (var apReport in reportsOfUser)
            {
                res.Items.Add(GetUserOneTopicSummary(apReport,false));
                if (apReport.user != null)
                    usrName = apReport.user.Name;
            }

            res.Header = "User summary for " + usrName;

            return res;
        }

        TreeViewItem GetAvgUserSummary(ArgArgPointReport report)
        {
            var txt = "  Number of points " + report.numPoints + "\n";
            txt += "  Number of points with description " + report.numPointsWithDescriptions + "\n";
            txt += "  Number of media attachments " + report.numMediaAttachments + "\n";
            txt += "  Number of sources " + report.numSources + "\n";
            txt += "  Number of comments " + report.numComments;

            var tb = new TextBlock();
            tb.Text = txt;
            var tvi = new TreeViewItem();
            tvi.Items.Add(tb);
            tvi.Header = "<average user, all topics>";
            return tvi;
        }

        public void reportGenerated()
        {
            txtLastSync.Text = DateTime.Now.ToString();
            
            topicSection.Items.Clear();
            foreach (var topicReport in _collector.TopicReports)
                topicSection.Items.Add(GetTopicReport(topicReport));

            var totals = _collector.AllTopicsReport;
            topicSection.Items.Add(GetTotalTopicSummary(totals));


            usersSection.Items.Clear();
            foreach(var report in _collector.ArgPointReports.Values)
                usersSection.Items.Add(GetUserSummary(report));

            usersSection.Items.Add(GetUserOneTopicSummary(_collector.TotalArgPointReport, true));

            usersSection.Items.Add(GetAvgUserSummary(_collector.AvgArgPointReport));

            participants.ItemsSource = _collector.Participants;

            discName.Text = _collector.Discussion.Subject;

            totalTime.Text = _collector.TotalDiscussionTime.ToString();
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
