using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Win32;

namespace Reporter
{
    public partial class ReporterWindow : Window
    {
        ReportCollector _reportCollector1 = null;
        ReportCollector _reportCollector2 = null;

        public ReporterWindow()
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

        void copyToClipboard(string text)
        {
            Clipboard.SetData(DataFormats.Text, text);
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

        TextBlock GetTopicSummary(TopicReport report, ReportParameters param, bool total)
        {
            var txt = "  Cumulative duration: " + TimeSpan.FromSeconds(report.cumulativeDuration) + "\r\n";

            var nUsr = 0;
            if (total)
                nUsr = report.GetNumAccumulatedParticipantsAmong();
            else
                nUsr = report.GetNumParticipantsAmong(param.requiredUsers);
            txt += "  No. of users: " + nUsr + "\r\n";
            txt += "  No. of arg. points: " + report.numPoints + "\r\n";
            txt += "  No. of arg. points with description: " + report.numPointsWithDescription + "\r\n";
            txt += "  No. of media attachments: " + report.numMediaAttachments + "\r\n";
            txt += "  No. of sources: " + report.numSources + "\r\n";
            txt += "  No. of comments: " + report.numComments + "\r\n";
            if (!total)
                txt += "  No. of clustered badges: " + report.numClusteredBadges + "\r\n";
            txt += "  No. of clusters: " + report.numClusters + "\r\n";
            txt += "  No. of links: " + report.numLinks;

            var res = new TextBlock();
            res.Text = txt;
            return res;
        }

        TextBlock GetAttachmentsSummary(ReportCollector report)
        {
            var txt = "<media summary>\r\n";
            txt += "No. of images " + report.NumImagesInSession + "\r\n";
            txt += "No. of PDF " + report.NumPdfInSession + "\r\n";
            txt += "No. of screenshots " + report.NumScreenshotsInSession + "\r\n";
            txt += "No. of videos " + report.NumYoutubeInSession;
            return StatsUtils.WrapText(txt);
        }

        TreeViewItem GetEvent(StatsEvent e, DiscCtx ctx)
        {
            var res = new TreeViewItem();
            var eventView = new EventViewModel((StEvent)e.Event, e.UserId, e.Time, (DeviceType)e.DeviceType);
            res.Header = eventView.evt;

            res.Items.Add(GetUser(ctx.Person.FirstOrDefault(p0 => p0.Id == e.UserId)));

            if (!string.IsNullOrEmpty(e.TopicName))
                res.Items.Add(e.TopicName);
            if (!string.IsNullOrEmpty(e.DiscussionName))
                res.Items.Add(e.DiscussionName);
            res.Items.Add(eventView.dateTime);
            res.Items.Add(eventView.devType);

            return res;
        }

        TextBlock GetUserEventTotals(EventUserReport eTotals)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<user event totals>");

            sb.Append("no. arg.point changed ");
            sb.AppendLine(eTotals.TotalArgPointTopicChanged.ToString());

            sb.Append("no. badge created ");
            sb.AppendLine(eTotals.TotalBadgeCreated.ToString());

            sb.Append("no. badge edited ");
            sb.AppendLine(eTotals.TotalBadgeEdited.ToString());

            sb.Append("no. badge moved ");
            sb.AppendLine(eTotals.TotalBadgeMoved.ToString());

            sb.Append("no. badge zoom in ");
            sb.AppendLine(eTotals.TotalBadgeZoomIn.ToString());

            sb.Append("no. cluster created ");
            sb.AppendLine(eTotals.TotalClusterCreated.ToString());

            sb.Append("no. cluster deleted ");
            sb.AppendLine(eTotals.TotalClusterDeleted.ToString());

            sb.Append("no. cluster-in ");
            sb.AppendLine(eTotals.TotalClusterIn.ToString());

            sb.Append("no. cluster moved ");
            sb.AppendLine(eTotals.TotalClusterMoved.ToString());

            sb.Append("no. cluster-out ");
            sb.AppendLine(eTotals.TotalClusterOut.ToString());

            sb.Append("no. comment added ");
            sb.AppendLine(eTotals.TotalCommentAdded.ToString());

            sb.Append("no. comment removed ");
            sb.AppendLine(eTotals.TotalCommentRemoved.ToString());

            sb.Append("no. free drawing created ");
            sb.AppendLine(eTotals.TotalFreeDrawingCreated.ToString());

            sb.Append("no. free drawing moved ");
            sb.AppendLine(eTotals.TotalFreeDrawingMoved.ToString());

            sb.Append("no. free drawing removed ");
            sb.AppendLine(eTotals.TotalFreeDrawingRemoved.ToString());

            sb.Append("no. free drawing resize ");
            sb.AppendLine(eTotals.TotalFreeDrawingResize.ToString());

            sb.Append("no. image added ");
            sb.AppendLine(eTotals.TotalImageAdded.ToString());

            sb.Append("no. image opened ");
            sb.AppendLine(eTotals.TotalImageOpened.ToString());

            sb.Append("no. image url added ");
            sb.AppendLine(eTotals.TotalImageUrlAdded.ToString());

            sb.Append("no. link created ");
            sb.AppendLine(eTotals.TotalLinkCreated.ToString());

            sb.Append("no. link removed ");
            sb.AppendLine(eTotals.TotalLinkRemoved.ToString());

            sb.Append("no. media removed ");
            sb.AppendLine(eTotals.TotalMediaRemoved.ToString());

            sb.Append("no. PDF added ");
            sb.AppendLine(eTotals.TotalPdfAdded.ToString());

            sb.Append("no. PDF opened ");
            sb.AppendLine(eTotals.TotalPdfOpened.ToString());

            sb.Append("no. PDF url added ");
            sb.AppendLine(eTotals.TotalPdfUrlAdded.ToString());

            sb.Append("no. source added ");
            sb.AppendLine(eTotals.TotalSourceAdded.ToString());

            sb.Append("no. source opened ");
            sb.AppendLine(eTotals.TotalSourceOpened.ToString());

            sb.Append("no. source removed ");
            sb.AppendLine(eTotals.TotalSourceRemoved.ToString());

            sb.Append("no. video opened ");
            sb.AppendLine(eTotals.TotalVideoOpened.ToString());

            sb.Append("no. video added ");
            sb.AppendLine(eTotals.TotalYoutubeAdded.ToString());

            sb.Append("no. scene zoom in ");
            sb.AppendLine(eTotals.TotalSceneZoomedIn.ToString());

            sb.Append("no. scene zoom out ");
            sb.AppendLine(eTotals.TotalSceneZoomedOut.ToString());

            sb.Append("no. screenshot added ");
            sb.AppendLine(eTotals.TotalScreenshotAdded.ToString());

            sb.Append("no. screenshot opened ");
            sb.AppendLine(eTotals.TotalScreenshotOpened.ToString());

            return StatsUtils.WrapText(sb.ToString());
        }

        TreeViewItem GetCluster(ClusterReport report)
        {
            var res = new TreeViewItem();
            res.Header = GetHeader(report.initialOwner, " - cluster " + report.clusterTitle);

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
            res.Header = GetHeader(report.initOwner, " - link");
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
            res.Items.Add(GetTopicSummary(report, collector.ReportParams, false));
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

        static TreeViewItem WrapNode(string nodeHeader)
        {
            var res = new TreeViewItem();
            res.Header = nodeHeader;
            return res;
        }

        TreeViewItem GetTotalTopicSummary(TopicReport report)
        {
            var res = new TreeViewItem();
            res.Header = "<all topics total>";
            res.Items.Add(GetTopicSummary(report, null, true));
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
            res.Header = GetHeader(ap.Person, " - " + ap.Point);
            res.Items.Add(txt);

            return res;
        }

        StackPanel GetHeader(Person pers, string str)
        {
            var res = new TreeViewItem();
            var st = new StackPanel();
            st.Orientation = Orientation.Horizontal;
            st.Children.Add(GetUser(pers));
            st.Children.Add(StatsUtils.WrapText(str));
            return st;
        }

        TreeViewItem GetUserOneTopicSummary(ArgPointReport report, bool totalUser)
        {
            var txt = "  No. of points " + report.numPoints + "\r\n";
            txt += "  No. of points with description " + report.numPointsWithDescriptions + "\r\n";
            txt += "  No. of media attachments " + report.numMediaAttachments + "\r\n";
            txt += "  No. of sources " + report.numSources + "\r\n";
            txt += "  No. of comments " + report.numComments;

            var tb = new TextBlock();
            tb.Text = txt;
            var tvi = new TreeViewItem();
            tvi.Items.Add(tb);

            if (!totalUser)
            {
                var usrId = report.user.Id;
                foreach (var ap in report.topic.ArgPoint.Where(ap0 => ap0.Person.Id == usrId))
                    tvi.Items.Add(GetPointReport(ap));
            }

            if (report.topic != null && !totalUser)
                tvi.Header = report.topic.Name;
            else
                tvi.Header = "<total user, all topics>";

            return tvi;
        }

        TreeViewItem GetUserSummary(List<ArgPointReport> reportsOfUser, EventUserReport eventUserReport)
        {
            var res = new TreeViewItem();
            string usrName = "";
            foreach (var apReport in reportsOfUser)
            {
                res.Items.Add(GetUserOneTopicSummary(apReport, false));
                if (apReport.user != null)
                    usrName = apReport.user.Name;
            }

            if (eventUserReport != null)
                res.Items.Add(GetUserEventTotals(eventUserReport));

            res.Header = "User summary for " + usrName;

            return res;
        }

        TreeViewItem GetAvgUserSummary(ArgArgPointReport report)
        {
            var txt = "  No. of points " + report.numPoints + "\r\n";
            txt += "  No. of points with description " + report.numPointsWithDescriptions + "\r\n";
            txt += "  No. of media attachments " + report.numMediaAttachments + "\r\n";
            txt += "  No. of sources " + report.numSources + "\r\n";
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
                usersNode = usersSection1;
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

            if (_reportCollector1 != null && _reportCollector2 != null)
            {
                var requiredUsers = StatsUtils.Union(reportHeader1.getReportParams(false).requiredUsers,
                                                     reportHeader2.getReportParams(false).requiredUsers);
                var totals = ReportCollector.GetTotalTopicsReports(_reportCollector1.TopicReports.First(),
                                                                   _reportCollector2.TopicReports.First(),
                                                                   requiredUsers);
                topicsNode.Items.Add(GetTotalTopicSummary(totals));
            }

            topicsNode.Items.Add(GetAttachmentsSummary(sender));

            topicsNode.Items.Add(StatsUtils.GetEventTotals(sender.EventTotals));

            usersNode.Items.Clear();
            foreach (var report in sender.ArgPointReports.Values)
            {
                EventUserReport eventReport = null;
                if (report.Count > 0)
                {
                    eventReport = sender.PerUserEventReportDict.ContainsKey(report.First().user.Id) ?
                                             sender.PerUserEventReportDict[report.First().user.Id] : null;
                }
                usersNode.Items.Add(GetUserSummary(report, eventReport));
            }

            eventsNode.Items.Clear();
            foreach (var ev in sender.StatsEvents)
                eventsNode.Items.Add(GetEvent(ev, sender.GetCtx()));
            usersNode.Items.Add(GetUserOneTopicSummary(sender.TotalArgPointReport, true));
        }

        LoginResult testLoginStub(DiscCtx ctx)
        {
            var loginRes = new LoginResult();
            loginRes.discussion = ctx.Discussion.First();
            loginRes.person = ctx.Person.FirstOrDefault(p0 => p0.Name.StartsWith("moder"));
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
            var ssv = (SurfaceScrollViewer)sender;
            ssv.ScrollToVerticalOffset(ssv.VerticalOffset - e.Delta);
        }

        private void btnSpss_Click(object sender, RoutedEventArgs e)
        {
            CsvExport(true);
        }

        private void CsvExport(bool perEvent)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV files (.csv)|*.csv";
            dlg.CheckFileExists = false;
            dlg.Title = "Report name and folder?";

            // Display OpenFileDialog by calling ShowDialog method
            bool? result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                var generated = false;

                if (_reportCollector1 != null && _reportCollector2 == null)
                {
                    generated = true;

                    if (perEvent)
                    {
                        CsvEventExporter.Export(dlg.FileName,
                                                reportHeader1.getReportParams(false),
                                                null);
                    }
                    else
                    {
                        CsvExporter.Export(dlg.FileName,
                                           _reportCollector1.TopicReports.First(),
                                           reportHeader1.getReportParams(false),
                                           _reportCollector1.EventTotals,
                                           null, null, null);
                    }
                }
                else if (_reportCollector1 == null && _reportCollector2 != null)
                {
                    generated = true;

                    if (perEvent)
                    {
                        CsvEventExporter.Export(dlg.FileName,
                                                reportHeader2.getReportParams(false),
                                                null);
                    }
                    else
                    {
                        CsvExporter.Export(dlg.FileName,
                                           _reportCollector2.TopicReports.First(),
                                           reportHeader2.getReportParams(false),
                                           _reportCollector2.EventTotals,
                                           null, null, null);
                    }
                }
                else if (_reportCollector1 != null && _reportCollector2 != null)
                {
                    generated = true;

                    if (perEvent)
                    {
                        CsvEventExporter.Export(dlg.FileName,
                                           reportHeader1.getReportParams(false),
                                           reportHeader2.getReportParams(false));
                    }
                    else
                    {
                        CsvExporter.Export(dlg.FileName,
                                           _reportCollector1.TopicReports.First(),
                                           reportHeader1.getReportParams(false),
                                           _reportCollector1.EventTotals,
                                        
                                          _reportCollector2.TopicReports.First(),
                                          reportHeader2.getReportParams(false),
                                          _reportCollector2.EventTotals);
                    }
                }
                var dirName = System.IO.Path.GetDirectoryName(dlg.FileName);
                if (generated)
                    try
                    {
                        Process.Start("explorer.exe", dirName);
                    }
                    catch (Exception)
                    { }
            }
        }

        string getIndentStr(int offset)
        {
            switch (offset)
            {
                case 0:
                    return "";
                case 1:
                    return " ";
                case 2:
                    return "  ";
                case 3:
                    return "   ";
                case 4:
                    return "    ";
                case 5:
                    return "     ";
            }
            return "      ";
        }

        void buildTextTree(object root, StringBuilder sb, int offset)
        {
            if (root == null)
                return;

            var child = root as TextBlock;
            if (child != null)
            {
                sb.AppendLine(getIndentStr(offset) + child.Text);
                return;
            }

            var childTvi = root as TreeViewItem;
            if (childTvi == null)
                return;

            var skipSubtree = false;
            var hdrStr = childTvi.Header as string;
            if (hdrStr != null)
            {
                sb.AppendLine(getIndentStr(offset) + hdrStr);
                if (hdrStr == "Links" || hdrStr == "Clusters")
                    skipSubtree = true;
            }

            var hdrStk = childTvi.Header as StackPanel;
            if (hdrStk != null)
            {
                var muc = hdrStk.Children[0] as MiniUserUC;
                if (muc != null)
                {
                    sb.AppendLine(getIndentStr(offset) + ((Person)muc.DataContext).Name);
                }
            }

            if (skipSubtree)
                return;

            foreach (var c in childTvi.Items)
                buildTextTree(c, sb, offset + 1);
        }

        private void MainWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var sb = new StringBuilder();
                buildTextTree(recentlySelectedLeftTree ? leftReportTree.SelectedItem : rightReportTree.SelectedItem, sb, 0);
                copyToClipboard(sb.ToString());
            }
        }

        bool recentlySelectedLeftTree = false;
        private void leftReportTree_SelectedItemChanged_1(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            recentlySelectedLeftTree = true;
        }

        private void rightReportTree_SelectedItemChanged_1(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            recentlySelectedLeftTree = false;
        }

        private void btnExcel_Click_1(object sender, RoutedEventArgs e)
        {
            CsvExport(false);
        }
    }
}
