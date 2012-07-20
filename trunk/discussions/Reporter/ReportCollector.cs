using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using System.Collections.ObjectModel;

namespace Reporter
{
    public class ReportCollector
    {
        int _discussionId;
        DiscCtx _ctx;
        Discussion _discussion;
        List<Topic> topics;

        int _processedTopicIdx = -1;
        int _clusterReportsGenerated = 0;

        public delegate void TopicReportReady(TopicReport topicReport);
        TopicReportReady _topicReportReady;

        //totals over all topics
        TopicReportReady _allTopicTotalsReady;

        TopicReport _allTopicsReport;

        Action _reportGenerated;

        ObservableCollection<TopicReport> _topicReports = new ObservableCollection<TopicReport>();
        public ObservableCollection<TopicReport> TopicReports
        {
            get
            {
                return _topicReports;
            }
            set
            {
                _topicReports = value; 
            }
        }

        ObservableCollection<ClusterReport> _clusterReports = new ObservableCollection<ClusterReport>();
        public ObservableCollection<ClusterReport> ClusterReports
        {
            get
            {
                return _clusterReports;
            }
            set
            {
                _clusterReports = value; 
            }
        }

        public ReportCollector(int discussionId, TopicReportReady topicReportReady, 
                               TopicReportReady allTopicTotals, Action reportGenerated)
        {
            _discussionId = discussionId;

            _ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);

            _discussion = _ctx.Discussion.FirstOrDefault(d0 => d0.Id == _discussionId);

            _topicReportReady = topicReportReady;

            _allTopicTotalsReady = allTopicTotals;

            _allTopicsReport = new TopicReport(null, 0, 0, 0, 0, 0, 0, 0, null);

            _reportGenerated = reportGenerated;

            setListeners(true);

            topics = _discussion.Topic.ToList();
            if (topics.Count() > 0)
            {
                UISharedRTClient.Instance.clienRt.dEditorReportResponse += dEditorReportResponse;
                _processedTopicIdx = 0;
                UISharedRTClient.Instance.clienRt.SendDEditorRequest(topics[_processedTopicIdx++].Id);
            }
            else
            {
                if (_allTopicTotalsReady != null)
                {
                    _allTopicTotalsReady(_allTopicsReport);                  
                }

                finalizeReport();
            }
        }

        void setListeners(bool doSet)
        {
            if (doSet)
            {
                UISharedRTClient.Instance.clienRt.clusterStatsResponse += clusterStatsResponse;             
            }
            else
            {
                UISharedRTClient.Instance.clienRt.clusterStatsResponse -= clusterStatsResponse;
            }
        }

        void clusterStatsResponse(ClusterStatsResponse resp, bool ok)
        {
            if (!ok)
            {
                if (++_clusterReportsGenerated == _allTopicsReport.numClusters)
                    finalizeReport();
                return;
            }
            
            //generate list if ArgPoints
            var argPoints = new ArgPoint[resp.points.Length];
            for (int i = 0; i < resp.points.Length; i++)
            {
                var pointId = resp.points[i];
                argPoints[i] = _ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == pointId);
            }

            var topic = _ctx.Topic.FirstOrDefault(t0 => t0.Id == resp.topicId);
            var report = new ClusterReport(topic, resp.clusterId, resp.clusterTextTitle, argPoints);
            _clusterReports.Add(report);

            if (++_clusterReportsGenerated == _allTopicsReport.numClusters)
            {
                finalizeReport();
            }
        }

        void finalizeReport()
        {
            setListeners(false);
            if (_reportGenerated != null)
                _reportGenerated();
        }

        void dEditorReportResponse(DEditorStatsResponse stats)
        {
            //process
            var topic = _ctx.Topic.FirstOrDefault(t0 => t0.Id == stats.TopicId);

            int numSrc;
            int numComments;       
            NumSources(topic, out numSrc, out numComments);            
           
            _allTopicsReport.numClusters        += stats.NumClusters;
            _allTopicsReport.numClusteredBadges += stats.NumClusteredBadges;
            _allTopicsReport.numLinks           += stats.NumLinks;
            _allTopicsReport.numParticipants    += topic.Person.Count();
            _allTopicsReport.numSources         += numSrc;
            _allTopicsReport.numComments        += numComments;
            _allTopicsReport.cumulativeDuration += topic.CumulativeDuration;

            var report = new TopicReport(topic, stats.NumClusters, stats.NumClusteredBadges,
                                         stats.NumLinks, topic.Person.Count(), numSrc, numComments,
                                         topic.CumulativeDuration, stats.ListOfClusterIds);

            _topicReports.Add(report);
            if (_topicReportReady!=null)
                _topicReportReady(report);            
   
            if (_processedTopicIdx > topics.Count() - 1)
            {
                UISharedRTClient.Instance.clienRt.dEditorReportResponse -= dEditorReportResponse;

                if (_allTopicTotalsReady != null)
                    _allTopicTotalsReady(_allTopicsReport);

                //all topics processed, request clusters 
                bool hasClusters = false;
                foreach(var topicReport in _topicReports)
                    foreach (var clustId in topicReport.clusterIds)
                    {
                        UISharedRTClient.Instance.clienRt.SendClusterStatsRequest(clustId, topicReport.topic.Id);
                        hasClusters = true;
                    }
                if (!hasClusters)
                    finalizeReport();
            }
            else
            {
                UISharedRTClient.Instance.clienRt.SendDEditorRequest(topics[_processedTopicIdx++].Id);  
            }
        }

        public static void NumSources(Topic topic, out int numSrc, out int numComments)
        {
            numSrc = 0;
            numComments = 0;
            foreach (var pt in topic.ArgPoint)
            {
                numSrc += pt.Description.Source.Count();
                numComments += pt.Comment.Count();
            }            
        }

        public string DiscussionSubject
        {
            get
            {    
                return _discussion.Subject;
            }
        }

        public Discussion Discussion
        {
            get
            {
                return _discussion;
            }
        }

        //public int TotalNumPoints
        //{
        //    get
        //    {
        //        var res = 0;
        //        foreach(var topic in _discussion.Topic)                
        //            res += topic.ArgPoint.Count();
        //        return res; 
        //    }
        //}

        //public int TotalGroups
        //{
        //    get
        //    {
        //        Total Group  Generated 
        //    }
        //}
    }
}
