﻿using System;
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
        int _linkReportsGenerated = 0;


        public delegate void TopicReportReady(TopicReport topicReport);
        TopicReportReady _topicReportReady;

        //totals over all topics
        TopicReportReady _allTopicTotalsReady;

        TopicReport _allTopicsReport;
        public TopicReport AllTopicsReport
        {
            get
            {
                return _allTopicsReport;
            }
        }

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

        ObservableCollection<LinkReportResponse> _linkReports = new ObservableCollection<LinkReportResponse>();
        public ObservableCollection<LinkReportResponse> LinkReports
        {
            get
            {
                return _linkReports;
            }
            set
            {
                _linkReports = value;
            }
        }

        //user id to arg point report
        Dictionary<int,ArgPointReport> _argPointReports = new Dictionary<int,ArgPointReport>();
        public Dictionary<int, ArgPointReport> ArgPointReports
        {
            get
            {
                return _argPointReports;
            }
            set
            {
                _argPointReports = value;
            }
        }

        public ArgPointReport TotalArgPointReport = new ArgPointReport();
        public ArgArgPointReport AvgArgPointReport = new ArgArgPointReport(); 

        public ReportCollector(int discussionId, TopicReportReady topicReportReady, 
                               TopicReportReady allTopicTotals, Action reportGenerated)
        {
            _discussionId = discussionId;

            _ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);

            _discussion = _ctx.Discussion.FirstOrDefault(d0 => d0.Id == _discussionId);

            _topicReportReady = topicReportReady;

            _allTopicTotalsReady = allTopicTotals;

            _allTopicsReport = new TopicReport(null, 0, 0, 0, new List<Person>(), 0, 0, 0, null, null);

            _reportGenerated = reportGenerated;

            setListeners(true);

            prepareArgPointReports();

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
                UISharedRTClient.Instance.clienRt.linkStatsResponseEvent += linkStatsResponse; 
            }
            else
            {
                UISharedRTClient.Instance.clienRt.clusterStatsResponse -= clusterStatsResponse;
                UISharedRTClient.Instance.clienRt.linkStatsResponseEvent -= linkStatsResponse; 
            }
        }

        bool ClustersAndLinksDone()
        {
            return _clusterReportsGenerated == _allTopicsReport.numClusters &&
                   _linkReportsGenerated == _allTopicsReport.numLinks;
        }

        void linkStatsResponse(LinkReportResponse resp, bool ok)
        {
            if (!ok)
            {
                ++_linkReportsGenerated;
                if (ClustersAndLinksDone())
                    finalizeReport();
                return;
            }

            if(resp.EndpointArgPoint1)
                resp.ArgPoint1 = _ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == resp.ArgPointId1);

            if (resp.EndpointArgPoint2)
                resp.ArgPoint2 = _ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == resp.ArgPointId2);

            var topic = _ctx.Topic.FirstOrDefault(t0 => t0.Id == resp.topicId);
            _linkReports.Add(resp);

            ++_linkReportsGenerated;
            if (ClustersAndLinksDone())
                finalizeReport();    
        }

        void clusterStatsResponse(ClusterStatsResponse resp, bool ok)
        {
            if (!ok)
            {
                ++_clusterReportsGenerated;
                if(ClustersAndLinksDone())
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

            ++_clusterReportsGenerated;
            if (ClustersAndLinksDone())                       
                finalizeReport();            
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
            ArgPointTotalsOverTopic(topic, out numSrc, out numComments);            
           
            _allTopicsReport.numClusters        += stats.NumClusters;
            _allTopicsReport.numClusteredBadges += stats.NumClusteredBadges;
            _allTopicsReport.numLinks           += stats.NumLinks;
            _allTopicsReport.participants.AddRange(topic.Person); 
            _allTopicsReport.numSources         += numSrc;
            _allTopicsReport.numComments        += numComments;
            _allTopicsReport.cumulativeDuration += topic.CumulativeDuration;

            var report = new TopicReport(topic, stats.NumClusters, stats.NumClusteredBadges,
                                         stats.NumLinks, topic.Person, numSrc, numComments,
                                         topic.CumulativeDuration, stats.ListOfClusterIds, 
                                         stats.ListOfLinkIds);

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
                bool hasLinks = false;
                foreach (var topicReport in _topicReports)
                {
                    foreach (var clustId in topicReport.clusterIds)
                    {
                        UISharedRTClient.Instance.clienRt.SendClusterStatsRequest(clustId, topicReport.topic.Id);
                        hasClusters = true;
                    }
                    foreach (var linkId in topicReport.linkIds)
                    {
                        UISharedRTClient.Instance.clienRt.SendLinkStatsRequest(linkId, topicReport.topic.Id);
                        hasLinks = true;
                    }
                }
                if (!hasClusters && !hasLinks)
                    finalizeReport();
            }
            else
            {
                UISharedRTClient.Instance.clienRt.SendDEditorRequest(topics[_processedTopicIdx++].Id);  
            }
        }

        public static void ArgPointTotalsOverTopic(Topic topic, out int numSrc, out int numComments)
        {
            numSrc = 0;
            numComments = 0;
            foreach (var pt in topic.ArgPoint)
            {
                numSrc += pt.Description.Source.Count();
                numComments += pt.Comment.Where(c0 => c0.Text != "New comment").Count();
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

        void prepareArgPointReports()
        {
            foreach (var topic in _discussion.Topic)            
                foreach (var ap in topic.ArgPoint)
                {
                    if (!ArgPointReports.ContainsKey(ap.Person.Id))                    
                        ArgPointReports.Add(ap.Person.Id, new ArgPointReport(0, 0, 0, 0, 0, ap.Person));

                    var report = ArgPointReports[ap.Person.Id];

                    //comments can be by different users
                    foreach (var c in ap.Comment)
                    {
                        if (c.Person == null)
                            continue; //for placeholders
                        
                        if (!ArgPointReports.ContainsKey(c.Person.Id))
                            ArgPointReports.Add(c.Person.Id, new ArgPointReport(0, 0, 0, 0, 0, c.Person));

                        if (c.Text != "New comment")
                            ArgPointReports[c.Person.Id].numComments += 1;                        
                    }

                    report.numMediaAttachments += ap.Attachment.Count();
                    report.numPoints += 1;
                    if (ap.Description.Text != "Description")
                        report.numPointsWithDescriptions++;
                    report.numSources += ap.Description.Source.Count();
                }   
         
            //total over all users
            foreach (var apReport in ArgPointReports.Values)
            {
                TotalArgPointReport.numComments += apReport.numComments;
                TotalArgPointReport.numMediaAttachments += apReport.numMediaAttachments;
                TotalArgPointReport.numPoints += apReport.numPoints;
                TotalArgPointReport.numPointsWithDescriptions += apReport.numPointsWithDescriptions;
                TotalArgPointReport.numSources += apReport.numSources;                
            }

            //avg
            var n = ArgPointReports.Count();
            AvgArgPointReport.numComments = (double)TotalArgPointReport.numComments / n;
            AvgArgPointReport.numMediaAttachments = (double)TotalArgPointReport.numMediaAttachments / n;
            AvgArgPointReport.numPoints = (double)TotalArgPointReport.numPoints / n;
            AvgArgPointReport.numPointsWithDescriptions = (double)TotalArgPointReport.numPointsWithDescriptions / n;
            AvgArgPointReport.numSources = (double)TotalArgPointReport.numSources / n;
        }
    }
}
