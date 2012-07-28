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

        ReportParameters parameters = null;

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

        ReportParameters getReportParams(bool forceDlg)
        {
            if (parameters != null && !forceDlg)
                return parameters;

            var dlg = new SessionTopicDlg();
            dlg.ShowDialog();
           
            if (dlg.reportParameters != null)
                sessionName.Text = dlg.reportParameters.session.Name;
            else
                sessionName.Text = null;

            return dlg.reportParameters;
        }

        public void onJoin()
        {
            UISharedRTClient.Instance.clienRt.onJoin -= onJoin;

            parameters = getReportParams(false);
            if (parameters != null)
            {
                _collector = new ReportCollector(null, null, reportGenerated, parameters);
            }
        }

        TextBlock GetTopicSummary(TopicReport report)
        {
            var txt = "  Cumulative duration: " + TimeSpan.FromSeconds(report.cumulativeDuration) + "\n";
            txt += "  No. of users: " + report.numParticipants + "\n";
            txt += "  No. of arg. points: " + report.numPoints + "\n";
            txt += "  No. of arg. points with description: " + report.numPointsWithDescription + "\n";
            txt += "  No. of media attachments: " + report.numMediaAttachments + "\n";
            txt += "  No. of sources: " + report.numSources + "\n";
            txt += "  No. of comments: " + report.numComments + "\n";
            txt += "  No. of clustered badges: " + report.numClusteredBadges + "\n";
            txt += "  No. of clusters: " + report.numClusters + "\n";
            txt += "  No. of links: " + report.numLinks;
          
            var res = new TextBlock();
            res.Text = txt;
            return res;
        }

        TreeViewItem GetCluster(ClusterReport report)
        {
            var res = new TreeViewItem();
            res.Header = "Cluster " + report.clusterTitle;
            
            res.Items.Add(GetUser(report.initialOwner));

            var argPoints = WrapNode("Arg. points");
            foreach (var ap in report.points)
                argPoints.Items.Add(GetPointReport(ap));

            res.Items.Add(argPoints);

            return res;
        }

        MiniUserUC GetUser(Person owner)
        {
            var usr = new MiniUserUC();
            usr.DataContext = owner;
            return usr;
        }

        TreeViewItem GetLink(LinkReportResponse report)
        {
            var res = new TreeViewItem();
            res.Header = "Link";
            res.Items.Add(GetUser(report.initOwner));

            var endpoints = WrapNode("Endpoints");

            if (report.EndpointArgPoint1)
                endpoints.Items.Add(GetPointReport(report.ArgPoint1));
            else
                endpoints.Items.Add(GetCluster(_collector.ClusterReports.FirstOrDefault(c0 => c0.clusterId == report.IdOfCluster1)));

            if (report.EndpointArgPoint2)
                endpoints.Items.Add(GetPointReport(report.ArgPoint2));
            else
                endpoints.Items.Add(GetCluster(_collector.ClusterReports.FirstOrDefault(c0 => c0.clusterId == report.IdOfCluster2)));

            res.Items.Add(endpoints);

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
            var clusters = WrapNode("Clusters");
            foreach (var clustReport in _collector.ClusterReports)
            {
                if (clustReport.topic.Id != report.topic.Id)
                    continue;

                clusters.Items.Add(GetCluster(clustReport));                
            }
            res.Items.Add(clusters);          

            //links            
            var links = WrapNode("Links");
            foreach (var linkReport in _collector.LinkReports)
            {
                if (linkReport.topicId != report.topic.Id)
                    continue;

                links.Items.Add(GetLink(linkReport));                           
            }            
            res.Items.Add(links);

            return res;
        }

        TreeViewItem WrapNode(string nodeHeader)
        {
            var res = new TreeViewItem();
            res.Header = nodeHeader;
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
                    
            res.Items.Add(GetUser(ap.Person));

            return res;
        }

        TreeViewItem GetUserOneTopicSummary(ArgPointReport report, bool totalUser)
        {
            var txt = "  No. of points " + report.numPoints + "\n";
            txt += "  No. of points with description " + report.numPointsWithDescriptions + "\n";
            txt += "  No. of media attachments " + report.numMediaAttachments + "\n";
            txt += "  No. of sources " + report.numSources + "\n";
            txt += "  No. of comments " + report.numComments;
           
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
            var txt = "  No. of points " + report.numPoints + "\n";
            txt += "  No. of points with description " + report.numPointsWithDescriptions + "\n";
            txt += "  No. of media attachments " + report.numMediaAttachments + "\n";
            txt += "  No. of sources " + report.numSources + "\n";
            txt += "  No. of comments " + report.numComments;

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

            sessionName.Text = parameters.session.Name;

            topicName.Text = parameters.topic.Name;

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
            parameters = getReportParams(false);
            if(parameters!=null)
                _collector = new ReportCollector(null, null, reportGenerated, parameters);
        }

        private void btnSelect_Click_1(object sender, RoutedEventArgs e)
        {
            parameters = null;
            btnRun_Click_1(null,null);
        }
    }
}
