using System;
using System.Collections.Generic;
using System.Linq;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using System.Collections.ObjectModel;

namespace Reporter
{
    public class ReportCollector
    {
        private DiscussionsClientRT.ClientRT _clientRT;

        private DiscCtx _ctx;
        private List<Topic> topics;

        private int _nProcessedTopics = 0;
        private int _clusterReportsGenerated = 0;
        private int _linkReportsGenerated = 0;

        private int _numImagesInSession = 0;

        public int NumImagesInSession
        {
            get { return _numImagesInSession; }
        }

        private int _numScreenshotsInSession = 0;

        public int NumScreenshotsInSession
        {
            get { return _numScreenshotsInSession; }
        }

        private int _numYoutubeInSession = 0;

        public int NumYoutubeInSession
        {
            get { return _numYoutubeInSession; }
        }

        private int _numPdfInSession = 0;

        public int NumPdfInSession
        {
            get { return _numPdfInSession; }
        }

        public delegate void TopicReportReady(TopicReport topicReport);

        private TopicReportReady _topicReportReady;

        private TopicReport _allTopicsReport;

        private ObservableCollection<TopicReport> _topicReports = new ObservableCollection<TopicReport>();

        public ObservableCollection<TopicReport> TopicReports
        {
            get { return _topicReports; }
            set { _topicReports = value; }
        }

        private ObservableCollection<ClusterReport> _clusterReports = new ObservableCollection<ClusterReport>();

        public ObservableCollection<ClusterReport> ClusterReports
        {
            get { return _clusterReports; }
            set { _clusterReports = value; }
        }

        private ObservableCollection<LinkReportResponse> _linkReports = new ObservableCollection<LinkReportResponse>();

        public ObservableCollection<LinkReportResponse> LinkReports
        {
            get { return _linkReports; }
            set { _linkReports = value; }
        }

        private ObservableCollection<StatsEvent> _statsEvents = new ObservableCollection<StatsEvent>();

        public ObservableCollection<StatsEvent> StatsEvents
        {
            get { return _statsEvents; }
            set { _statsEvents = value; }
        }

        private Dictionary<int, EventUserReport> perUserEventReport = new Dictionary<int, EventUserReport>();

        public IEnumerable<EventUserReport> PerUserEventReport
        {
            get { return perUserEventReport.Values; }
        }

        public Dictionary<int, EventUserReport> PerUserEventReportDict
        {
            get { return perUserEventReport; }
        }

        //user id to arg point reports of the user, grouped by topic
        private Dictionary<int, List<ArgPointReport>> _argPointReports = new Dictionary<int, List<ArgPointReport>>();

        public Dictionary<int, List<ArgPointReport>> ArgPointReports
        {
            get { return _argPointReports; }
            set { _argPointReports = value; }
        }

        public ArgPointReport TotalArgPointReport = new ArgPointReport();
        public ArgArgPointReport AvgArgPointReport = new ArgArgPointReport();

        private ReportParameters _reportParams;

        public ReportParameters ReportParams
        {
            get { return _reportParams; }
        }

        private EventTotalsReport _eventTotals = new EventTotalsReport();

        public EventTotalsReport EventTotals
        {
            get { return _eventTotals; }
        }

        public delegate void ReportReady(ReportCollector sender, object args);

        private ReportReady _reportGenerated;

        public object _param = null;

        public ReportCollector(TopicReportReady topicReportReady,
                               ReportReady reportGenerated,
                               ReportParameters reportParams,
                               object param, DiscussionsClientRT.ClientRT clientRt)
        {
            _reportParams = reportParams;

            _param = param;
            _clientRT = clientRt;

            _ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);

            _topicReportReady = topicReportReady;

            _allTopicsReport = new TopicReport(null, 0, 0, 0, 0, 0, 0, null, null, 0, 0, 0, 0, 0, 0, 0);

            _reportGenerated = reportGenerated;

            setListeners(true);

            topics = new List<Topic>() {_reportParams.topic};

            prepareArgPointReports();

            prepareEvents();

            if (topics.Count() > 0)
            {
                _clientRT.dEditorReportResponse += dEditorReportResponse;
                _clientRT.SendDEditorRequest(topics.First().Id);
            }
            else
            {
                finalizeReport();
            }
        }

        private void setListeners(bool doSet)
        {
            if (doSet)
            {
                _clientRT.clusterStatsResponse += clusterStatsResponse;
                _clientRT.linkStatsResponseEvent += linkStatsResponse;
            }
            else
            {
                _clientRT.clusterStatsResponse -= clusterStatsResponse;
                _clientRT.linkStatsResponseEvent -= linkStatsResponse;
            }
        }

        public DiscCtx GetCtx()
        {
            return _ctx;
        }

        private bool ClustersAndLinksDone()
        {
            return _clusterReportsGenerated == _allTopicsReport.numClusters &&
                   _linkReportsGenerated == _allTopicsReport.numLinks;
        }

        private void linkStatsResponse(LinkReportResponse resp, bool ok)
        {
            if (!ok)
            {
                ++_linkReportsGenerated;
                if (ClustersAndLinksDone())
                    finalizeReport();
                return;
            }

            if (resp.EndpointArgPoint1)
                resp.ArgPoint1 = _ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == resp.ArgPointId1);

            if (resp.EndpointArgPoint2)
                resp.ArgPoint2 = _ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == resp.ArgPointId2);

            var topic = _ctx.Topic.FirstOrDefault(t0 => t0.Id == resp.topicId);
            resp.initOwner = _ctx.Person.FirstOrDefault(p0 => p0.Id == resp.initialOwner);
            _linkReports.Add(resp);

            ++_linkReportsGenerated;
            if (ClustersAndLinksDone())
                finalizeReport();
        }

        private void clusterStatsResponse(ClusterStatsResponse resp, bool ok)
        {
            if (!ok)
            {
                ++_clusterReportsGenerated;
                if (ClustersAndLinksDone())
                    finalizeReport();
                return;
            }

            //generate list of ArgPoints
            var argPoints = new ArgPoint[resp.points.Length];
            for (int i = 0; i < resp.points.Length; i++)
            {
                var pointId = resp.points[i];
                argPoints[i] = _ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == pointId);
            }

            var topic = _ctx.Topic.FirstOrDefault(t0 => t0.Id == resp.topicId);
            var initialOwner = _ctx.Person.FirstOrDefault(p0 => p0.Id == resp.initialOwnerId);
            var report = new ClusterReport(topic, resp.clusterId, resp.clusterShId, resp.clusterTextTitle, argPoints,
                                           initialOwner);
            _clusterReports.Add(report);

            ++_clusterReportsGenerated;
            if (ClustersAndLinksDone())
                finalizeReport();
        }

        private void finalizeReport()
        {
            setListeners(false);
            if (_reportGenerated != null)
                _reportGenerated(this, _param);
        }

        public static TopicReport GetTotalTopicsReports(TopicReport topic1, TopicReport topic2,
                                                        List<int> sessionParticipants)
        {
            var res = new TopicReport();
            var topicsDifferent = topic1.topic.Id != topic2.topic.Id;

            if (topicsDifferent)
                res.cumulativeDuration = topic1.cumulativeDuration + topic2.cumulativeDuration;
            else
                res.cumulativeDuration = topic1.cumulativeDuration;

            //what to report?
            ///res.numClusteredBadges += topic1.numClusteredBadges + topic2.numClusteredBadges;

            //participants
            res.accumulatedParticipants = new List<int>();
            foreach (var usr in topic1.topic.Person)
                if (!res.accumulatedParticipants.Contains(usr.Id) && sessionParticipants.Contains(usr.Id))
                    res.accumulatedParticipants.Add(usr.Id);
            foreach (var usr in topic2.topic.Person)
                if (!res.accumulatedParticipants.Contains(usr.Id) && sessionParticipants.Contains(usr.Id))
                    res.accumulatedParticipants.Add(usr.Id);

            //number of unique clusters
            var clusterIds = new List<int>();
            foreach (var cid in topic1.clusterIds)
                if (!clusterIds.Contains(cid))
                    clusterIds.Add(cid);
            foreach (var cid in topic2.clusterIds)
                if (!clusterIds.Contains(cid))
                    clusterIds.Add(cid);
            res.numClusters = clusterIds.Count();

            //number of unique links
            var linkIds = new List<int>();
            foreach (var lid in topic1.linkIds)
                if (!linkIds.Contains(lid))
                    linkIds.Add(lid);
            foreach (var lid in topic2.linkIds)
                if (!linkIds.Contains(lid))
                    linkIds.Add(lid);
            res.numLinks = linkIds.Count();

            //number of unique points
            var uniquePoints = new List<ArgPoint>();
            foreach (var ap in topic1.topic.ArgPoint)
                if (!uniquePoints.Contains(ap))
                    uniquePoints.Add(ap);
            res.numPoints = uniquePoints.Count();

            foreach (var ap in uniquePoints)
            {
                if (ap.Description.Text != "Description")
                    res.numPointsWithDescription += 1;

                foreach (var c in ap.Comment)
                    if (c.Text != "New feedback" && res.accumulatedParticipants.Contains(c.Person.Id))
                        ++res.numComments;

                foreach (var a in ap.Attachment)
                    if (sessionParticipants.Contains(a.Person.Id))
                        ++res.numMediaAttachments;

                if (res.accumulatedParticipants.Contains(ap.Person.Id))
                    res.numSources += ap.Description.Source.Count();
            }

            return res;
        }

        private void dEditorReportResponse(DEditorStatsResponse stats)
        {
            //process
            var topic = _ctx.Topic.FirstOrDefault(t0 => t0.Id == stats.TopicId);

            int numSrc;
            int numComments;
            int numImages;
            int numScreenshots;
            int numPdfs;
            int numYoutubes;
            ArgPointTotalsOverTopic(_reportParams, topic, out numSrc, out numComments,
                                    out numImages, out numPdfs, out numScreenshots, out numYoutubes);

            _allTopicsReport.numClusters += stats.NumClusters;
            _allTopicsReport.numClusteredBadges += stats.NumClusteredBadges;
            _allTopicsReport.numLinks += stats.NumLinks;
            _allTopicsReport.numSources += numSrc;
            _allTopicsReport.numComments += numComments;
            _allTopicsReport.numImages += numImages;
            _allTopicsReport.numPDFs += numPdfs;
            _allTopicsReport.numYoutubes += numYoutubes;
            _allTopicsReport.numScreenshots += numScreenshots;
            _allTopicsReport.cumulativeDuration += topic.CumulativeDuration;
            var numArgPoints = topic.ArgPoint.Count();
            _allTopicsReport.numPoints += numArgPoints;
            var numPointsWithDescription = topic.ArgPoint.Where(ap0 => ap0.Description != null && ap0.Description.Text != "Description").Count();
            _allTopicsReport.numPointsWithDescription += numPointsWithDescription;
            //num media
            var nMedia = 0;
            foreach (var ap in topic.ArgPoint)
                nMedia += ap.Attachment.Count();
            _allTopicsReport.numMediaAttachments += nMedia;

            var report = new TopicReport(topic, stats.NumClusters, stats.NumClusteredBadges,
                                         stats.NumLinks, numSrc, numComments,
                                         topic.CumulativeDuration, stats.ListOfClusterIds,
                                         stats.ListOfLinkIds, numArgPoints,
                                         numPointsWithDescription, nMedia,
                                         numImages, numPdfs, numScreenshots, numYoutubes);

            _topicReports.Add(report);
            if (_topicReportReady != null)
                _topicReportReady(report);

            if (++_nProcessedTopics >= topics.Count())
            {
                _clientRT.dEditorReportResponse -= dEditorReportResponse;

                //all topics processed, request clusters 
                bool hasClusters = false;
                bool hasLinks = false;
                foreach (var topicReport in _topicReports)
                {
                    foreach (var clustId in topicReport.clusterIds)
                    {
                        _clientRT.SendClusterStatsRequest(clustId, topicReport.topic.Id);
                        hasClusters = true;
                    }
                    foreach (var linkId in topicReport.linkIds)
                    {
                        _clientRT.SendLinkStatsRequest(linkId, topicReport.topic.Id);
                        hasLinks = true;
                    }
                }
                if (!hasClusters && !hasLinks)
                    finalizeReport();
            }
            //else
            //{
            //    UISharedRTClient.Instance.clienRt.SendDEditorRequest(topics[nextTopicIdx++].Id);  
            //}
        }

        public static void ArgPointTotalsOverTopic(ReportParameters par, Topic topic,
                                                   out int numSrc, out int numComments, out int numImages,
                                                   out int numPdfs, out int numScreenshots, out int numYoutubes)
        {
            numSrc = 0;
            numComments = 0;
            numImages = 0;
            numPdfs = 0;
            numScreenshots = 0;
            numYoutubes = 0;
            foreach (var pt in topic.ArgPoint)
            {
                if (par.requiredUsers.Contains(pt.Person.Id))
                {
                    numSrc += pt.Description.Source.Count();

                    foreach (var a in pt.Attachment)
                    {
                        switch ((AttachmentFormat) a.Format)
                        {
                            case AttachmentFormat.None:
                                break;
                            case AttachmentFormat.Jpg:
                                numImages++;
                                break;
                            case AttachmentFormat.Png:
                                numImages++;
                                break;
                            case AttachmentFormat.Bmp:
                                numImages++;
                                break;
                            case AttachmentFormat.Pdf:
                                numPdfs++;
                                break;
                            case AttachmentFormat.Youtube:
                                numYoutubes++;
                                break;
                            case AttachmentFormat.GeneralWebLink:
                                break;
                            case AttachmentFormat.PngScreenshot:
                                numScreenshots++;
                                break;
                            case AttachmentFormat.WordDocSet:
                                break;
                            case AttachmentFormat.ExcelDocSet:
                                break;
                            case AttachmentFormat.PowerPointDocSet:
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    }
                }

                foreach (var c in pt.Comment)
                {
                    if (c.Person == null)
                        return;

                    if (c.Text != "New feedback" && par.requiredUsers.Contains(c.Person.Id))
                        ++numComments;
                }
            }
        }

        public TimeSpan TotalDiscussionTime
        {
            get
            {
                TimeSpan res = new TimeSpan(0);
                foreach (var t in topics)
                    if (_reportParams.topic.Id == t.Id)
                        res.Add(TimeSpan.FromSeconds(t.CumulativeDuration));
                return res;
            }
        }

        public ObservableCollection<Person> Participants
        {
            get
            {
                var res = new ObservableCollection<Person>();
                foreach (var t in topics)
                    foreach (var usr in t.Person)
                        if (_reportParams.requiredUsers.Contains(usr.Id) && !res.Contains(usr))
                            res.Add(usr);
                return res;
            }
        }

        private ArgPointReport FindTopicArgPointReport(List<ArgPointReport> reports, int topicId)
        {
            foreach (var r in reports)
                if (r.topic != null && r.topic.Id == topicId)
                    return r;
            return null;
        }

        private void prepareArgPointReports()
        {
            foreach (var topic in topics)
                foreach (var ap in topic.ArgPoint)
                {
                    //comments can be by different users
                    foreach (var c in ap.Comment)
                    {
                        if (c.Person == null)
                            continue; //for placeholders

                        if (!_reportParams.requiredUsers.Contains(c.Person.Id))
                            continue;

                        if (!ArgPointReports.ContainsKey(c.Person.Id))
                            ArgPointReports.Add(c.Person.Id, new List<ArgPointReport>());
                        var reportsOfCommenter = ArgPointReports[c.Person.Id];

                        if (c.Text != "New feedback")
                        {
                            var topicReportOfCommenter = FindTopicArgPointReport(reportsOfCommenter, ap.Topic.Id);
                            if (topicReportOfCommenter == null)
                            {
                                topicReportOfCommenter = new ArgPointReport(0, 0, 0, 0, 0, c.Person, topic);
                                reportsOfCommenter.Add(topicReportOfCommenter);
                            }

                            topicReportOfCommenter.numComments += 1;
                        }
                    }

                    foreach (var at in ap.Attachment)
                    {
                        if (!_reportParams.requiredUsers.Contains(at.Person.Id))
                            continue;

                        switch ((AttachmentFormat) at.Format)
                        {
                            case AttachmentFormat.Bmp:
                                ++_numImagesInSession;
                                break;
                            case AttachmentFormat.Jpg:
                                ++_numImagesInSession;
                                break;
                            case AttachmentFormat.Png:
                                ++_numImagesInSession;
                                break;
                            case AttachmentFormat.Pdf:
                                ++_numPdfInSession;
                                break;
                            case AttachmentFormat.PngScreenshot:
                                ++_numScreenshotsInSession;
                                break;
                            case AttachmentFormat.Youtube:
                                ++_numYoutubeInSession;
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    }

                    if (!_reportParams.requiredUsers.Contains(ap.Person.Id))
                        continue;

                    if (!ArgPointReports.ContainsKey(ap.Person.Id))
                        ArgPointReports.Add(ap.Person.Id, new List<ArgPointReport>());
                    var reportsInTopic = ArgPointReports[ap.Person.Id];

                    var topicReportOfSelf = FindTopicArgPointReport(reportsInTopic, ap.Topic.Id);
                    if (topicReportOfSelf == null)
                    {
                        topicReportOfSelf = new ArgPointReport(0, 0, 0, 0, 0, ap.Person, topic);
                        reportsInTopic.Add(topicReportOfSelf);
                    }

                    topicReportOfSelf.numMediaAttachments += ap.Attachment.Count();
                    topicReportOfSelf.numPoints += 1;
                    if (ap.Description.Text != "Description")
                        topicReportOfSelf.numPointsWithDescriptions++;
                    topicReportOfSelf.numSources += ap.Description.Source.Count();
                }

            //fill out users who don't have points
            foreach (var topic in topics)
            {
                foreach (var p in topic.Person)
                {
                    if (!_reportParams.requiredUsers.Contains(p.Id))
                        continue;

                    if (!ArgPointReports.ContainsKey(p.Id))
                        ArgPointReports.Add(p.Id, new List<ArgPointReport>());

                    var topicReports = ArgPointReports[p.Id];
                    var topicReportOfPers = FindTopicArgPointReport(topicReports, topic.Id);
                    if (topicReportOfPers == null)
                    {
                        topicReports.Add(new ArgPointReport(0, 0, 0, 0, 0, p, topic));
                    }
                }
            }

            //total over all users
            foreach (var apReport in ArgPointReports.Values)
                foreach (var userTopicReport in apReport)
                {
                    TotalArgPointReport.numComments += userTopicReport.numComments;
                    TotalArgPointReport.numMediaAttachments += userTopicReport.numMediaAttachments;
                    TotalArgPointReport.numPoints += userTopicReport.numPoints;
                    TotalArgPointReport.numPointsWithDescriptions += userTopicReport.numPointsWithDescriptions;
                    TotalArgPointReport.numSources += userTopicReport.numSources;
                }

            //avg
            var n = ArgPointReports.Count();
            AvgArgPointReport.numComments = (double) TotalArgPointReport.numComments/n;
            AvgArgPointReport.numMediaAttachments = (double) TotalArgPointReport.numMediaAttachments/n;
            AvgArgPointReport.numPoints = (double) TotalArgPointReport.numPoints/n;
            AvgArgPointReport.numPointsWithDescriptions = (double) TotalArgPointReport.numPointsWithDescriptions/n;
            AvgArgPointReport.numSources = (double) TotalArgPointReport.numSources/n;
        }

        private void prepareEvents()
        {
            foreach (var e in _ctx.StatsEvent)
            {
                if (e.TopicId != -1 && e.TopicId != _reportParams.topic.Id)
                    continue;

                if (e.UserId != -1 && !_reportParams.requiredUsers.Contains(e.UserId))
                    continue;

                StatsEvents.Add(e);
                EventTotals.CountEvent((StEvent) e.Event, e.Id);

                //count per user stats 
                if (e.UserId != -1)
                {
                    if (perUserEventReport.ContainsKey(e.UserId))
                        perUserEventReport[e.UserId].CountEvent((StEvent) e.Event, e.Id);
                    else
                    {
                        var report = new EventUserReport();
                        report.personId = e.UserId;
                        perUserEventReport.Add(e.UserId, report);
                        report.CountEvent((StEvent) e.Event, e.Id);
                    }
                }
            }

            foreach (var e in PerUserEventReport)
            {
                e.person = _ctx.Person.FirstOrDefault(p0 => p0.Id == e.personId);
            }
        }
    }
}