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
using Discussions.stats;
using Microsoft.Surface.Presentation.Controls;

namespace Reporter
{   
    public partial class MainWindow : Window
    {
        ReportCollector _reportCollector1 = null;
        ReportCollector _reportCollector2 = null;

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

            reportHeader1.ParamsChanged += Report1ParamsChanged;
            reportHeader2.ParamsChanged += Report2ParamsChanged;
        }

        void Report1ParamsChanged(ReportParameters param)
        {
            if (param != null)
                new ReportCollector(null, reportGenerated, param, leftReportTree);
        }

        void Report2ParamsChanged(ReportParameters param)
        {
            if (param != null)
                new ReportCollector(null, reportGenerated, param, rightReportTree);
        }

        public void onJoin()
        {
            UISharedRTClient.Instance.clienRt.onJoin -= onJoin;       
        }

        TextBlock GetTopicSummary(TopicReport report, bool total)
        {
            var txt = "  Cumulative duration: " + TimeSpan.FromSeconds(report.cumulativeDuration) + "\n";

            var nUsr = 0;
            if (total)
                nUsr = report.GetNumParticipantsAmong2(reportHeader1.getReportParams(false).requiredUsers);
            else
                nUsr = report.GetNumParticipantsAmong(reportHeader1.getReportParams(false).requiredUsers);
            txt += "  No. of users: " + nUsr + "\n";
            txt += "  No. of arg. points: " + report.numPoints + "\n";
            txt += "  No. of arg. points with description: " + report.numPointsWithDescription + "\n";
            txt += "  No. of media attachments: " + report.numMediaAttachments + "\n";
            txt += "  No. of sources: " + report.numSources + "\n";
            txt += "  No. of comments: " + report.numComments + "\n";
            if (!total)
                txt += "  No. of clustered badges: " + report.numClusteredBadges + "\n";
            txt += "  No. of clusters: " + report.numClusters + "\n";
            txt += "  No. of links: " + report.numLinks;
          
            var res = new TextBlock();
            res.Text = txt;
            return res;
        }

        TreeViewItem GetAttachmentsSummary(ReportCollector report)
        {
            var res = new TreeViewItem();
            res.Header = "<media summary>";
            res.Items.Add("No. of images "      + report.NumImagesInSession);
            res.Items.Add("No. of PDF "         + report.NumPdfInSession);
            res.Items.Add("No. of screenshots " + report.NumScreenshotsInSession);
            res.Items.Add("No. of videos "      + report.NumYoutubeInSession);  
            return res; 
        }

        TreeViewItem GetEvent(StatsEvent e, DiscCtx ctx)
        {
            var res = new TreeViewItem();
            var eventView = new EventViewModel((StEvent)e.Event, e.UserId, e.Time, (DeviceType)e.DeviceType);
            res.Header = eventView.evt;

            res.Items.Add(GetUser(ctx.Person.FirstOrDefault(p0 => p0.Id == e.UserId)));

            if (!string.IsNullOrEmpty(e.TopicName))
                res.Items.Add(e.TopicName); 
            if(!string.IsNullOrEmpty(e.DiscussionName))
                res.Items.Add(e.DiscussionName); 
            res.Items.Add(eventView.dateTime);
            res.Items.Add(eventView.devType);            

            return res;
        }

        TreeViewItem GetEventTotals(EventTotalsReport eTotals)
        {
            var res = new TreeViewItem();
            res.Header = "<event totals>";

            res.Items.Add("no. arg.point changed " +  eTotals.TotalArgPointTopicChanged);
            res.Items.Add("no. badge created " +  eTotals.TotalBadgeCreated);
            res.Items.Add("no. badge edited " + eTotals.TotalBadgeEdited);
            res.Items.Add("no. badge moved " + eTotals.TotalBadgeMoved);
            res.Items.Add("no. badge zoom in " + eTotals.TotalBadgeZoomIn);
            res.Items.Add("no. cluster created " + eTotals.TotalClusterCreated);
            res.Items.Add("no. cluster deleted " + eTotals.TotalClusterDeleted);
            res.Items.Add("no. cluster-in " + eTotals.TotalClusterIn);
            res.Items.Add("no. cluster moved " + eTotals.TotalClusterMoved);
            res.Items.Add("no. cluster-out " + eTotals.TotalClusterOut);
            res.Items.Add("no. comment added " + eTotals.TotalCommentAdded);
            res.Items.Add("no. comment removed " + eTotals.TotalCommentRemoved);
            res.Items.Add("no. free drawing created " + eTotals.TotalFreeDrawingCreated);
            res.Items.Add("no. free drawing moved " + eTotals.TotalFreeDrawingMoved);
            res.Items.Add("no. free drawing removed " + eTotals.TotalFreeDrawingRemoved);
            res.Items.Add("no. free drawing resize " + eTotals.TotalFreeDrawingResize);
            res.Items.Add("no. image added " + eTotals.TotalImageAdded);
            res.Items.Add("no. image opened " + eTotals.TotalImageOpened);
            res.Items.Add("no. image url added " + eTotals.TotalImageUrlAdded);
            res.Items.Add("no. link created " + eTotals.TotalLinkCreated);
            res.Items.Add("no. link removed " + eTotals.TotalLinkRemoved);
            res.Items.Add("no. media removed " + eTotals.TotalMediaRemoved);
            res.Items.Add("no. PDF added " + eTotals.TotalPdfAdded);
            res.Items.Add("no. PDF opened " + eTotals.TotalPdfOpened);
            res.Items.Add("no. PDF url added " + eTotals.TotalPdfUrlAdded);
            res.Items.Add("no. source added " + eTotals.TotalSourceAdded);
            res.Items.Add("no. source opened " + eTotals.TotalSourceOpened);
            res.Items.Add("no. source removed " + eTotals.TotalSourceRemoved);
            res.Items.Add("no. video opened " + eTotals.TotalVideoOpened);
            res.Items.Add("no. video added " + eTotals.TotalYoutubeAdded);
            res.Items.Add("no. recording started " + eTotals.TotalRecordingStarted);
            res.Items.Add("no. recording stopped " + eTotals.TotalRecordingStopped);
            res.Items.Add("no. scene zoom in " + eTotals.TotalSceneZoomedIn);
            res.Items.Add("no. scene zoom out " + eTotals.TotalSceneZoomedOut);
            res.Items.Add("no. screenshot added " + eTotals.TotalScreenshotAdded);
            res.Items.Add("no. screenshot opened " + eTotals.TotalScreenshotOpened);

            return res;
        }

        TreeViewItem GetCluster(ClusterReport report)
        {
            var res = new TreeViewItem();
            res.Header = GetHeader(report.initialOwner, " cluster " + report.clusterTitle);

            var argPoints = WrapNode("Arg. points");
            foreach (var ap in report.points)
                argPoints.Items.Add(GetPointReport(ap));

            res.Items.Add(argPoints);

            return res;
        }

        MiniUserUC GetUser(Person owner)
        {
            var usr = new MiniUserUC();

            if (owner == null)
            {
                owner = new Person();
                owner.Name = "SYSTEM";
                owner.Color = Utils2.ColorToInt(Colors.Aqua);                                
            }

            usr.DataContext = owner;
            return usr;
        }

        TreeViewItem GetLink(LinkReportResponse report, ReportCollector collector)
        {
            var res = new TreeViewItem();
            res.Header = GetHeader(report.initOwner, " link");
            var endpoints = WrapNode("Endpoints");

            if (report.EndpointArgPoint1)
                endpoints.Items.Add(GetPointReport(report.ArgPoint1));
            else
                endpoints.Items.Add(GetCluster(collector.ClusterReports.FirstOrDefault(c0 => c0.clusterId == report.IdOfCluster1)));

            if (report.EndpointArgPoint2)
                endpoints.Items.Add(GetPointReport(report.ArgPoint2));
            else
                endpoints.Items.Add(GetCluster(collector.ClusterReports.FirstOrDefault(c0 => c0.clusterId == report.IdOfCluster2)));

            res.Items.Add(endpoints);

            return res;
        }

        TextBlock GetTextBlock(string txt)
        {
            var res = new TextBlock();
            res.Text = txt;
            return res;        
        }

        TreeViewItem GetTopicReport(TopicReport report, ReportCollector collector)
        {            
            var res = new TreeViewItem();
            res.Items.Add(GetTopicSummary(report, false));
            res.Header = report.topic.Name;

            //clusters
            var clusters = WrapNode("Clusters");
            foreach (var clustReport in collector.ClusterReports)
            {
                if (clustReport.topic.Id != report.topic.Id)
                    continue;

                clusters.Items.Add(GetCluster(clustReport));                
            }
            res.Items.Add(clusters);          

            //links            
            var links = WrapNode("Links");
            foreach (var linkReport in collector.LinkReports)
            {
                if (linkReport.topicId != report.topic.Id)
                    continue;

                links.Items.Add(GetLink(linkReport, collector));                           
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

        TextBlock WrapText(string txt)
        {
            var res = new TextBlock();
            res.Text = txt;
            return res;
        }

        TreeViewItem GetTotalTopicSummary(TopicReport report)
        {
            var res = new TreeViewItem();
            res.Header = "<all topics total>";            
            res.Items.Add(GetTopicSummary(report, true));
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
            res.Header = GetHeader(ap.Person, " arg. point " + ap.Point);
            res.Items.Add(txt);
                    
            return res;
        }

        StackPanel GetHeader(Person pers, string str)
        {
            var res = new TreeViewItem();
            var st = new StackPanel();
            st.Orientation = Orientation.Horizontal;
            st.Children.Add(GetUser(pers));
            st.Children.Add(WrapText(str));
            return st;           
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
       
        public void reportGenerated(ReportCollector sender, object args)
        {
            txtLastSync.Text = DateTime.Now.ToString();

            TreeViewItem topicsNode = null;
            TreeViewItem usersNode = null;
            TreeViewItem eventsNode = null;
            if (args == leftReportTree)
            {
                _reportCollector1 = sender;
                topicsNode = topicSection1;
                usersNode  = usersSection1;
                eventsNode = eventSection1;
                reportHeader1.SetParticipants(sender.Participants);
            }
            else if (args == rightReportTree)
            {
                _reportCollector2 = sender;
                topicsNode = topicSection2;
                usersNode = usersSection2;
                eventsNode = eventSection2;
                reportHeader2.SetParticipants(sender.Participants);
            }
            else
                throw new NotSupportedException();

            topicsNode.Items.Clear();
            foreach (var topicReport in sender.TopicReports)
                topicsNode.Items.Add(GetTopicReport(topicReport, sender));

            if(_reportCollector1!=null && _reportCollector2!=null)
            {
                var requiredUsers = StatsUtils.Union(reportHeader1.getReportParams(false).requiredUsers, 
                                                     reportHeader2.getReportParams(false).requiredUsers);
                var totals = ReportCollector.GetTotalTopicsReports(_reportCollector1.TopicReports.First(),
                                                                   _reportCollector2.TopicReports.First(),
                                                                   requiredUsers);
                topicsNode.Items.Add(GetTotalTopicSummary(totals));
            }

            topicsNode.Items.Add(GetAttachmentsSummary(sender));

            topicsNode.Items.Add(GetEventTotals(sender.EventTotals));

            usersNode.Items.Clear();
            foreach (var report in sender.ArgPointReports.Values)
                usersNode.Items.Add(GetUserSummary(report));

            eventsNode.Items.Clear();
            foreach (var ev in sender.StatsEvents)
                eventsNode.Items.Add(GetEvent(ev, sender.GetCtx()));
            usersNode.Items.Add(GetUserOneTopicSummary(sender.TotalArgPointReport, true)); 
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
            var parameters1 = reportHeader1.getReportParams(false);
            var parameters2 = reportHeader2.getReportParams(false);
            if (parameters1 == null || parameters2 == null)
                return;
            
            new ReportCollector(null, reportGenerated, parameters1, leftReportTree);
            new ReportCollector(null, reportGenerated, parameters2, rightReportTree);
        }

        private void SurfaceScrollViewer_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            var ssv =(SurfaceScrollViewer)sender;
            ssv.ScrollToVerticalOffset(ssv.VerticalOffset - e.Delta);
        }
    }
}
